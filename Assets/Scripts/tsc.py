import re
import sys
from enum import Enum
from pathlib import Path

logo = r'''
               .:-=++.                             
          .*@@@##%@=.                              
      .=@@###:#@#.                 ...             
    .*@@@@=-%@@@@@@@@@@.           .@@.            
  .-@@@@%-%#=----@@=---.           .@@.            
  ...#@#-@@@@*. .@@.    -@@@@@=%@: .@@.  .=@@@@@-. 
 -@@@@@+@@@@-   .@@.  .%@%:..:%@@: .@@. .@@-...=@%.
.%@@@@#@#%@*.   .@@.  =@#.    :%@: .@@. @@######@@-
..  .#*   .     .@@.  =@#.    :%@: .@@. @@-........
    :%          .@@.  .%@%.  :%@@: .@@. .@@+.  .*: 
   :@*          .@@.    =%@@@#=%@: .@@.  .+%@@@@*-.
  .#@=                                             
                  CarbonCopy Tale Script Compiler
---------------------------------------------------
'''

print(logo)

argc = len(sys.argv)

if argc != 2:
    print(f'Usage: python {sys.argv[0]} <file>')
    sys.exit(1)

file = sys.argv[1]

print(f'Compiling script {file}\n')

# Used so that 'Scene_X' or 'Scena_X' will be written to 'Story_X.cs'
scene_map = (re.compile(r'^(scen[ae])_.*$', re.IGNORECASE), 'Story')

class State(Enum):
    SEARCHING_FOR_ENTRY_POINT = 0,
    PARSING_SCRIPT = 1

def line_is_h1(line):
    return line.startswith('# ')

def line_to_h1(line):
     return line[1:].strip()

def line_is_h2(line):
    return line.startswith('## ')

def line_to_h2(line):
     return line[2:].strip()

def error(what):
    print(f'[ERROR] {what}', file=sys.stderr)
    sys.exit(1)

dialog_regex = re.compile(r'^\s*([a-zA-Z0-9]+):(.*)$')

def parse_dialog(line):
    dialog = dialog_regex.match(line)

    if dialog == None:
        return None
    
    who = dialog.group(1).strip()
    what = dialog.group(2).strip(' "').replace('"', '\\"')

    if len(what) == 0:
        return None
    
    return (who, what)

def write_scene_script_start(file, scene):
    file.write(f'''using UnityEngine;

public class {scene} : MonoBehavior {{
    void Awake() {{
        Tale.Wait();
        Transition.FadeIn();
''')

def write_scene_script_end(file):
    file.write('''     
        Tale.Wait();
        Transition.FadeOut();
        Tale.Scene();
    }
}
''')
    
def write_scene_script_dialog(file, who, what):
    file.write(f'''
        Dialog.{who}("{what}"''')
        
    if what.startswith('(') and what.endswith(')'):
        file.write(', reverb: true')

    file.write(''');
''')
    
def write_scene_script_wait(file):
    file.write('''
        Tale.Wait();
''')
    
def write_scene_script_comment(file, what):
    file.write(f'''
        // {what}
''')

state = State.SEARCHING_FOR_ENTRY_POINT
scene = None
scene_file = None

scenes = set()
characters = set()

with open(file) as f:
    for line in f:
        match state:
            case State.SEARCHING_FOR_ENTRY_POINT:
                if line_is_h1(line) and line_to_h1(line).lower() == 'script':
                    print('Found script entry point\n')
                    state = State.PARSING_SCRIPT
            case State.PARSING_SCRIPT:
                if line_is_h2(line):
                    text = line_to_h2(line)

                    print(f'[SCENE] {text}')

                    if scene_file != None:
                        write_scene_script_end(scene_file)
                        scene_file.close()
                        scene_file = None

                    # Normalize
                    scene = re.sub(r'[\s\-,\.;:\'"\[\]{}/\\!@#$%^&*]', '_', text)
                    scene = re.sub(r'_+', '_', scene).strip('_')

                    # Rename according to config
                    mapped = scene_map[0].sub(lambda s: s.group(0).replace(s.group(1), scene_map[1]), scene)

                    if scene != mapped:
                        scene = mapped
                        print(f'   ---> {scene}')

                    if scene in scenes:
                        error(f'Scene {scene} is already defined')

                    scenes.add(scene)

                    Path('Scenes').mkdir(exist_ok=True)
                    scene_file = open(f'Scenes/{scene}.cs', 'w')

                    write_scene_script_start(scene_file, scene)

                elif line_is_h1(line):
                    break # End of Script heading
                elif scene != None:
                    dialog = parse_dialog(line)

                    if dialog:
                        who = dialog[0]
                        what = dialog[1]

                        characters.add(who)

                        write_scene_script_dialog(scene_file, who, what)
                    elif line.strip() == '---':
                            write_scene_script_wait(scene_file)
                    elif len(line.strip()) > 0:
                        write_scene_script_comment(scene_file, line.strip())
                        

if state == State.SEARCHING_FOR_ENTRY_POINT:
    error("Failed to find entry point; make sure a '# Script' heading exists")

if scene_file != None:
    write_scene_script_end(scene_file)
    scene_file.close()
    scene_file = None

print('---')

print('Writing Transition.cs')

with open('Transition.cs', 'w') as f:
    f.write('''using UnityEngine;

public static class Transition {
    public static TaleUtil.Action FadeIn(float duration = 1f) =>
        Tale.Transition("fade", Tale.TransitionType.IN, duration);

    public static TaleUtil.Action FadeOut(float duration = 1f) =>
        Tale.Transition("fade", Tale.TransitionType.OUT, duration);
}
''')
    
print('Writing Dialog.cs')

with open('Dialog.cs', 'w') as f:
    f.write('''using UnityEngine;
            
public static class Dialog {''')

    for character in characters:
        f.write(f'''
    public static TaleUtil.Action {character}(string what, string voice = null, bool additive = false, bool reverb = false) =>
        Tale.Dialog("{character}", what, null, voice, voice.ToLower().EndsWith("Loop"), additive, reverb);
''')

    f.write('''
}
''')

print('\nCompiled successfully')
sys.exit(0)
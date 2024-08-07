namespace TaleUtil
{
    public static class Input
    {
        public static bool Advance()
        {
            if (UnityEngine.Input.GetMouseButtonUp(0) || UnityEngine.Input.GetKey(Config.DIALOG_KEY_SKIP))
                return true;

            for (int i = 0; i < Config.DIALOG_KEY_NEXT.Length; ++i)
                if (UnityEngine.Input.GetKeyDown(Config.DIALOG_KEY_NEXT[i]))
                    return true;

            return false;
        }
    }
}
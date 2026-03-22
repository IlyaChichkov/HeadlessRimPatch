using UnityEngine;
using Verse;

namespace HeadlessRim
{
    [StaticConstructorOnStartup]
    public static class HeadlessStartup
    {
        static HeadlessStartup()
        {
            Log.Message("[HeadlessRim] HeadlessStartup called. Locking framerate.");

            // Keeps the headless server from consuming 100% CPU on an uncapped thread
            Application.targetFrameRate = 30;
        }
    }
}
﻿using System.Collections.Generic;

namespace vimage
{
    public enum Action
    {
        None,

        Drag,
        Close,
        OpenContextMenu,
        PrevImage,
        NextImage,

        RotateClockwise,
        RotateAntiClockwise,
        Flip,
        FitToMonitorHeight,
        FitToMonitorWidth,
        FitToMonitorAuto,
        FitToMonitorAlt,
        ZoomIn,
        ZoomOut,
        ZoomFaster,
        ZoomAlt,
        DragLimitToMonitorBounds,

        ToggleSmoothing,
        ToggleMipmapping,
        ToggleBackground,
        ToggleLock,
        ToggleAlwaysOnTop,
        ToggleTitleBar,

        PauseAnimation,
        PrevFrame,
        NextFrame,

        OpenSettings,
        ResetImage,
        OpenAtLocation,
        Delete,
        Copy,
        CopyAsImage,
        OpenDuplicateImage,
        OpenFullDuplicateImage,
        RandomImage,

        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown,

        TransparencyToggle,
        TransparencyInc,
        TransparencyDec,
        Crop,
        UndoCrop,
        ExitAll,

        VersionName,

        SortName,
        SortDate,
        SortDateModified,
        SortDateCreated,
        SortSize,
        SortAscending,
        SortDescending
    }

    public static class Actions
    {
        public static List<string> Names = new List<string>
        {
            "",

            "DRAG",
            "CLOSE",
            "OPEN CONTEXT MENU",
            "PREV IMAGE",
            "NEXT IMAGE",

            "ROTATE CLOCKWISE",
            "ROTATE ANTICLOCKWISE",
            "FLIP",
            "FIT TO HEIGHT",
            "FIT TO WIDTH",
            "FIT TO AUTO",
            "FIT TO ALT",
            "ZOOM IN",
            "ZOOM OUT",
            "ZOOM FASTER",
            "ZOOM ALT",
            "DRAG LIMIT TO MONITOR BOUNDS",

            "TOGGLE SMOOTHING",
            "TOGGLE MIPMAPPING",
            "TOGGLE BACKGROUND",
            "TOGGLE LOCK",
            "ALWAYS ON TOP",
            "TOGGLE TITLE BAR",

            "TOGGLE ANIMATION",
            "PREV FRAME",
            "NEXT FRAME",

            "OPEN SETTINGS",
            "RESET IMAGE",
            "OPEN FILE LOCATION",
            "DELETE",
            "COPY",
            "COPY AS IMAGE",
            "OPEN DUPLICATE",
            "OPEN DUPLICATE FULL",
            "RANDOM IMAGE",

            "MOVE LEFT",
            "MOVE RIGHT",
            "MOVE UP",
            "MOVE DOWN",

            "TOGGLE IMAGE TRANSPARENCY",
            "TRANSPARENCY INC",
            "TRANSPARENCY DEC",
            "CROP",
            "UNDO CROP",
            "EXIT ALL INSTANCES",

            "VERSION NAME",

            "SORT NAME",
            "SORT DATE",
            "SORT DATE MODIFIED",
            "SORT DATE CREATED",
            "SORT SIZE",
            "SORT ASCENDING",
            "SORT DESCENDING"
        };

        public static string ToNameString(this Action action) { return Names[(int)action]; }
        public static Action StringToAction(string action) { return (Action)Names.IndexOf(action); }

        /// <summary>List of actions that can be used in the Context Menu.</summary>
        public static readonly Action[] MenuActions =
        {
            Action.Close, Action.NextImage, Action.PrevImage, Action.RotateClockwise, Action.RotateAntiClockwise,
            Action.Flip, Action.FitToMonitorHeight, Action.FitToMonitorWidth, Action.FitToMonitorAuto, Action.ResetImage,
            Action.ToggleSmoothing, Action.ToggleMipmapping, Action.ToggleBackground, Action.TransparencyToggle, Action.ToggleLock, Action.ToggleAlwaysOnTop, Action.ToggleTitleBar,
            Action.OpenAtLocation, Action.Delete, Action.Copy, Action.CopyAsImage,
            Action.OpenDuplicateImage, Action.OpenFullDuplicateImage, Action.RandomImage, Action.UndoCrop, Action.ExitAll,
            Action.PauseAnimation, Action.NextFrame, Action.PrevFrame,
            Action.OpenSettings, Action.VersionName,
            Action.SortName, Action.SortDate, Action.SortDateModified, Action.SortDateCreated, Action.SortSize,
            Action.SortAscending, Action.SortDescending,
        };
    }
}

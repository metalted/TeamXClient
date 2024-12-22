using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamXClient
{
    public enum TeamXPanelComponentName { Background, Save, Load, LoadHere, LoadFile, Home, Reload, LoadPreview, SavePreview, UpOneLevel, NewFolder, Upload, OpenFolder, Exit, ScrollView, URL, FileName, TypeText, SearchBar, Download, Search, PreviousPage, NextPage, PageCounter, SelectedName, SearchResultScrollView, PermissionEntryUser, PermissionEntryBanned, PermissionEntryGuest, PermissionEntryDefault, PermissionEntryTrusted, PermissionEntryAdmin };
    public enum TeamXPanelComponentType { Button, Image, Text, ScrollView, TextInput };
    public enum TeamXPanelState { Closed, Open };

    public class TeamXPanel : MonoBehaviour
    {

    }
}

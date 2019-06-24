/*
 * Copyright 2019 faddenSoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Windows;

namespace SourceGenWPF.Res {
    /// <summary>
    /// This is a bridge between the XAML definitions and the C# code that uses the strings.
    /// FindResource() throws an exception if the resource isn't found, so typos and missing
    /// resources will cause the app to fail at launch.
    /// </summary>
    public static class Strings {
        public static string ASM_LATEST_VERSION =
            (string)Application.Current.FindResource("str_AsmLatestVersion");
        public static string ASM_MATCH_FAILURE =
            (string)Application.Current.FindResource("str_AsmMatchFailure");
        public static string ASM_MATCH_SUCCESS =
            (string)Application.Current.FindResource("str_AsmMatchSuccess");
        public static string ASM_MISMATCH_CAPTION =
            (string)Application.Current.FindResource("str_AsmMismatchCaption");
        public static string ASM_MISMATCH_DATA_FMT =
            (string)Application.Current.FindResource("str_AsmMismatchDataFmt");
        public static string ASM_MISMATCH_LENGTH_FMT =
            (string)Application.Current.FindResource("str_AsmMismatchLengthFmt");
        public static string ASM_OUTPUT_NOT_FOUND =
            (string)Application.Current.FindResource("str_AsmOutputNotFound");
        public static string DEFAULT_VALUE =
            (string)Application.Current.FindResource("str_DefaultValue");
        public static string ERR_BAD_FD =
            (string)Application.Current.FindResource("str_ErrBadFd");
        public static string ERR_BAD_FD_FORMAT =
            (string)Application.Current.FindResource("str_ErrBadFdFormat");
        public static string ERR_BAD_FILE_LENGTH =
            (string)Application.Current.FindResource("str_ErrBadFileLength");
        public static string ERR_BAD_IDENT =
            (string)Application.Current.FindResource("str_ErrBadIdent");
        public static string ERR_BAD_RANGE =
            (string)Application.Current.FindResource("str_ErrBadRange");
        public static string ERR_BAD_SYMBOL_ST =
            (string)Application.Current.FindResource("str_ErrBadSymbolSt");
        public static string ERR_BAD_SYMREF_PART =
            (string)Application.Current.FindResource("str_ErrBadSymrefPart");
        public static string ERR_BAD_TYPE_HINT =
            (string)Application.Current.FindResource("str_ErrBadTypeHint");
        public static string ERR_DUPLICATE_LABEL_FMT =
            (string)Application.Current.FindResource("str_ErrDuplicateLabelFmt");
        public static string ERR_FILE_EXISTS_NOT_DIR_FMT =
            (string)Application.Current.FindResource("str_ErrFileExistsNotDirFmt");
        public static string ERR_FILE_GENERIC_CAPTION =
            (string)Application.Current.FindResource("str_ErrFileGenericCaption");
        public static string ERR_FILE_NOT_FOUND_FMT =
            (string)Application.Current.FindResource("str_ErrFileNotFoundFmt");
        public static string ERR_FILE_READ_ONLY_FMT =
            (string)Application.Current.FindResource("str_ErrFileReadOnlyFmt");
        public static string ERR_INVALID_INT_VALUE =
            (string)Application.Current.FindResource("str_ErrInvalidIntValue");
        public static string ERR_INVALID_KEY_VALUE =
            (string)Application.Current.FindResource("str_ErrInvalidKeyValue");
        public static string ERR_INVALID_SYSDEF =
            (string)Application.Current.FindResource("str_ErrInvalidSysdef");
        public static string ERR_LOAD_CONFIG_FILE =
            (string)Application.Current.FindResource("str_ErrLoadConfigFile");
        public static string ERR_NOT_PROJECT_FILE =
            (string)Application.Current.FindResource("str_ErrNotProjectFile");
        public static string ERR_PROJECT_FILE_CORRUPT =
            (string)Application.Current.FindResource("str_ErrProjectFileCorrupt");
        public static string ERR_PROJECT_LOAD_FAIL =
            (string)Application.Current.FindResource("str_ErrProjectLoadFail");
        public static string ERR_PROJECT_SAVE_FAIL =
            (string)Application.Current.FindResource("str_ErrProjectSaveFail");
        public static string ERR_TOO_LARGE_FOR_PREVIEW =
            (string)Application.Current.FindResource("str_ErrTooLargeForPreview");
        public static string FILE_FILTER_ALL =
            (string)Application.Current.FindResource("str_FileFilterAll");
        public static string FILE_FILTER_CS =
            (string)Application.Current.FindResource("str_FileFilterCs");
        public static string FILE_FILTER_DIS65 =
            (string)Application.Current.FindResource("str_FileFilterDis65");
        public static string FILE_FILTER_SYM65 =
            (string)Application.Current.FindResource("str_FileFilterSym65");
        public static string FILE_INFO_FMT =
            (string)Application.Current.FindResource("str_FileInfoFmt");
        public static string GENERATED_FOR_VERSION_FMT =
            (string)Application.Current.FindResource("str_GeneratedForVersion");
        public static string INFO_FD_SUM_FMT =
            (string)Application.Current.FindResource("str_InfoFdSumFmt");
        public static string INFO_LINE_SUM_NON_FMT =
            (string)Application.Current.FindResource("str_InfoLineSumNonFmt");
        public static string INFO_LINE_SUM_PLURAL_FMT =
            (string)Application.Current.FindResource("str_InfoLineSumPluralFmt");
        public static string INFO_LINE_SUM_SINGULAR_FMT =
            (string)Application.Current.FindResource("str_InfoLineSumSingularFmt");
        public static string INITIAL_EXTENSION_SCRIPTS =
            (string)Application.Current.FindResource("str_InitialExtensionScripts");
        public static string INITIAL_PARAMETERS =
            (string)Application.Current.FindResource("str_InitialParameters");
        public static string INITIAL_SYMBOL_FILES =
            (string)Application.Current.FindResource("str_InitialSymbolFiles");
        public static string NO_FILES_AVAILABLE =
            (string)Application.Current.FindResource("str_NoFilesAvailable");
        public static string OPEN_DATA_DOESNT_EXIST =
            (string)Application.Current.FindResource("str_OpenDataDoesntExist");
        public static string OPEN_DATA_EMPTY =
            (string)Application.Current.FindResource("str_OpenDataEmpty");
        public static string OPEN_DATA_FAIL_CAPTION =
            (string)Application.Current.FindResource("str_OpenDataFailCaption");
        public static string OPEN_DATA_FAIL_MESSAGE =
            (string)Application.Current.FindResource("str_OpenDataFailMessage");
        public static string OPEN_DATA_PARTIAL_READ =
            (string)Application.Current.FindResource("str_OpenDataPartialRead");
        public static string OPEN_DATA_LOAD_FAILED_FMT =
            (string)Application.Current.FindResource("str_OpenDataLoadFailedFmt");
        public static string OPEN_DATA_TOO_LARGE_FMT =
            (string)Application.Current.FindResource("str_OpenDataTooLargeFmt");
        public static string OPEN_DATA_WRONG_CRC_FMT =
            (string)Application.Current.FindResource("str_OpenDataWrongCrcFmt");
        public static string OPEN_DATA_WRONG_LENGTH_FMT =
            (string)Application.Current.FindResource("str_OpenDataWrongLengthFmt");
        public static string OPERATION_FAILED =
            (string)Application.Current.FindResource("str_OperationFailed");
        public static string PLUGIN_DIR_FAIL_FMT =
            (string)Application.Current.FindResource("str_PluginDirFailFmt");
        public static string PLUGIN_DIR_FAIL_CAPTION =
            (string)Application.Current.FindResource("str_PluginDirFailCaption");
        public static string PROGRESS_ASSEMBLING =
            (string)Application.Current.FindResource("str_ProgressAssembling");
        public static string PROGRESS_GENERATING_FMT =
            (string)Application.Current.FindResource("str_ProgressGeneratingFmt");
        public static string PROJECT_FIELD_COMMENT =
            (string)Application.Current.FindResource("str_ProjectFieldComment");
        public static string PROJECT_FIELD_LONG_COMMENT =
            (string)Application.Current.FindResource("str_ProjectFieldLongComment");
        public static string PROJECT_FIELD_NOTE =
            (string)Application.Current.FindResource("str_ProjectFieldNote");
        public static string PROJECT_FIELD_OPERAND_FORMAT =
            (string)Application.Current.FindResource("str_ProjectFieldOperandFormat");
        public static string PROJECT_FIELD_STATUS_FLAGS =
            (string)Application.Current.FindResource("str_ProjectFieldStatusFlags");
        public static string PROJECT_FIELD_TYPE_HINT =
            (string)Application.Current.FindResource("str_ProjectFieldTypeHint");
        public static string PROJECT_FIELD_USER_LABEL =
            (string)Application.Current.FindResource("str_ProjectFieldUserLabel");
        public static string PROJECT_FROM_NEWER_APP =
            (string)Application.Current.FindResource("str_ProjectFromNewerApp");
        //public static string RECENT_PROJECT_LINK_FMT =
        //    (string)Application.Current.FindResource("str_RecentProjectLinkFmt");
        public static string RUNTIME_DIR_NOT_FOUND =
            (string)Application.Current.FindResource("str_RuntimeDirNotFound");
        public static string RUNTIME_DIR_NOT_FOUND_CAPTION =
            (string)Application.Current.FindResource("str_RuntimeDirNotFoundCaption");
        public static string SAVE_BEFORE_ASM =
            (string)Application.Current.FindResource("str_SaveBeforeAsm");
        public static string SAVE_BEFORE_ASM_CAPTION =
            (string)Application.Current.FindResource("str_SaveBeforeAsmCaption");
        public static string SETUP_SYSTEM_SUMMARY_FMT =
            (string)Application.Current.FindResource("str_SetupSystemSummaryFmt");
    }
}

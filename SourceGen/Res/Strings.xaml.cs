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

namespace SourceGen.Res {
    /// <summary>
    /// This is a bridge between the XAML definitions and the C# code that uses the strings.
    /// FindResource() throws an exception if the resource isn't found, so typos and missing
    /// resources will cause the app to fail the first time any string is referenced.
    /// </summary>
    public static class Strings {
        public static string ABBREV_ADDRESS =
            (string)Application.Current.FindResource("str_AbbrevAddress");
        public static string ABBREV_CONSTANT =
            (string)Application.Current.FindResource("str_AbbrevConstant");
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
        public static string DEFAULT_HEADER_COMMENT_FMT =
            (string)Application.Current.FindResource("str_DefaultHeaderCommentFmt");
        public static string DEFAULT_VALUE =
            (string)Application.Current.FindResource("str_DefaultValue");
        public static string DEFAULT_ASCII_DELIM_PAT =
            (string)Application.Current.FindResource("str_DefaultAsciiDelimPat");
        public static string DEFAULT_HIGH_ASCII_DELIM_PAT =
            (string)Application.Current.FindResource("str_DefaultHighAsciiDelimPat");
        public static string DEFAULT_C64_PETSCII_DELIM_PAT =
            (string)Application.Current.FindResource("str_DefaultC64PetsciiDelimPat");
        public static string DEFAULT_C64_SCREEN_CODE_DELIM_PAT =
            (string)Application.Current.FindResource("str_DefaultC64ScreenCodeDelimPat");
        public static string CLIPFORMAT_ASSEMBLER_SOURCE =
            (string)Application.Current.FindResource("str_ClipformatAssemblerSource");
        public static string CLIPFORMAT_DISASSEMBLY =
            (string)Application.Current.FindResource("str_ClipformatDisassembly");
        public static string ERR_BAD_FD_FMT =
            (string)Application.Current.FindResource("str_ErrBadFdFmt");
        public static string ERR_BAD_FD_FORMAT =
            (string)Application.Current.FindResource("str_ErrBadFdFormat");
        public static string ERR_BAD_FILE_LENGTH =
            (string)Application.Current.FindResource("str_ErrBadFileLength");
        public static string ERR_BAD_IDENT =
            (string)Application.Current.FindResource("str_ErrBadIdent");
        public static string ERR_BAD_LV_TABLE_FMT =
            (string)Application.Current.FindResource("str_ErrBadLvTableFmt");
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
        public static string EXTERNAL_FILE_BAD_DIR_FMT =
            (string)Application.Current.FindResource("str_ExternalFileBadDirFmt");
        public static string EXTERNAL_FILE_BAD_DIR_CAPTION =
            (string)Application.Current.FindResource("str_ExternalFileBadDirCaption");
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
        public static string FIND_REACHED_START =
            (string)Application.Current.FindResource("str_FindReachedStart");
        public static string FIND_REACHED_START_CAPTION =
            (string)Application.Current.FindResource("str_FindReachedStartCaption");
        public static string FONT_DESCRIPTOR_FMT =
            (string)Application.Current.FindResource("str_FontDescriptorFmt");
        public static string GENERATED_FOR_VERSION_FMT =
            (string)Application.Current.FindResource("str_GeneratedForVersion");
        public static string HIDE_COL =
            (string)Application.Current.FindResource("str_HideCol");
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
        public static string INVALID_ADDRESS =
            (string)Application.Current.FindResource("str_InvalidAddress");
        public static string INVALID_FORMAT_WORD_SEL_CAPTION =
            (string)Application.Current.FindResource("str_InvalidFormatWordSelCaption");
        public static string INVALID_FORMAT_WORD_SEL_NON1 =
            (string)Application.Current.FindResource("str_InvalidFormatWordSelNon1");
        public static string INVALID_FORMAT_WORD_SEL_UNEVEN_FMT =
            (string)Application.Current.FindResource("str_InvalidFormatWordSelUnevenFmt");
        public static string LOCAL_VARIABLE_TABLE_CLEAR =
            (string)Application.Current.FindResource("str_LocalVariableTableClear");
        public static string LOCAL_VARIABLE_TABLE_EMPTY =
            (string)Application.Current.FindResource("str_LocalVariableTableEmpty");
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
        public static string PARENTHETICAL_NONE =
            (string)Application.Current.FindResource("str_ParentheticalNone");
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
        public static string PROJECT_FIELD_LV_TABLE =
            (string)Application.Current.FindResource("str_ProjectFieldLvTable");
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
        public static string SCAN_LOW_ASCII =
            (string)Application.Current.FindResource("str_ScanLowAscii");
        public static string SCAN_LOW_HIGH_ASCII =
            (string)Application.Current.FindResource("str_ScanLowHighAscii");
        public static string SCAN_C64_PETSCII =
            (string)Application.Current.FindResource("str_ScanC64Petscii");
        public static string SCAN_C64_SCREEN_CODE =
            (string)Application.Current.FindResource("str_ScanC64ScreenCode");
        public static string SETUP_SYSTEM_SUMMARY_FMT =
            (string)Application.Current.FindResource("str_SetupSystemSummaryFmt");
        public static string SHOW_COL =
            (string)Application.Current.FindResource("str_ShowCol");
        public static string STATUS_READY =
            (string)Application.Current.FindResource("str_StatusReady");
        public static string SYMBOL_IMPORT_CAPTION =
            (string)Application.Current.FindResource("str_SymbolImportCaption");
        public static string SYMBOL_IMPORT_GOOD_FMT =
            (string)Application.Current.FindResource("str_SymbolImportGoodFmt");
        public static string SYMBOL_IMPORT_NONE =
            (string)Application.Current.FindResource("str_SymbolImportNone");
        public static string TITLE_BASE =
            (string)Application.Current.FindResource("str_TitleBase");
        public static string TITLE_MODIFIED =
            (string)Application.Current.FindResource("str_TitleModified");
        public static string TITLE_NEW_PROJECT =
            (string)Application.Current.FindResource("str_TitleNewProject");
        public static string UNSET =
            (string)Application.Current.FindResource("str_Unset");
    }
}

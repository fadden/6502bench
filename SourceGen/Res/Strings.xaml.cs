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
        public static string ABBREV_STACK_RELATIVE =
            (string)Application.Current.FindResource("str_AbbrevStackRelative");
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
        public static string DATA_BANK_AUTO_FMT =
            (string)Application.Current.FindResource("str_DataBankAutoFmt");
        public static string DATA_BANK_USER_FMT =
            (string)Application.Current.FindResource("str_DataBankUserFmt");
        public static string DATA_BANK_USER_K =
            (string)Application.Current.FindResource("str_DataBankUserK");
        public static string DEFAULT_HEADER_COMMENT_FMT =
            (string)Application.Current.FindResource("str_DefaultHeaderCommentFmt");
        public static string DEFAULT_ASCII_DELIM_PAT =
            (string)Application.Current.FindResource("str_DefaultAsciiDelimPat");
        public static string DEFAULT_HIGH_ASCII_DELIM_PAT =
            (string)Application.Current.FindResource("str_DefaultHighAsciiDelimPat");
        public static string DEFAULT_C64_PETSCII_DELIM_PAT =
            (string)Application.Current.FindResource("str_DefaultC64PetsciiDelimPat");
        public static string DEFAULT_C64_SCREEN_CODE_DELIM_PAT =
            (string)Application.Current.FindResource("str_DefaultC64ScreenCodeDelimPat");
        public static string CLIPFORMAT_ALL_COLUMNS =
            (string)Application.Current.FindResource("str_ClipformatAllColumns");
        public static string CLIPFORMAT_ASSEMBLER_SOURCE =
            (string)Application.Current.FindResource("str_ClipformatAssemblerSource");
        public static string CLIPFORMAT_DISASSEMBLY =
            (string)Application.Current.FindResource("str_ClipformatDisassembly");
        public static string EQU_ADDRESS =
            (string)Application.Current.FindResource("str_EquAddress");
        public static string EQU_CONSTANT =
            (string)Application.Current.FindResource("str_EquConstant");
        public static string EQU_STACK_RELATIVE =
            (string)Application.Current.FindResource("str_EquStackRelative");
        public static string ERR_BAD_DEF_SYMBOL_DIR =
            (string)Application.Current.FindResource("str_ErrBadDefSymbolDir");
        public static string ERR_BAD_FD_FMT =
            (string)Application.Current.FindResource("str_ErrBadFdFmt");
        public static string ERR_BAD_FD_FORMAT =
            (string)Application.Current.FindResource("str_ErrBadFdFormat");
        public static string ERR_BAD_FILE_LENGTH =
            (string)Application.Current.FindResource("str_ErrBadFileLength");
        public static string ERR_BAD_IDENT =
            (string)Application.Current.FindResource("str_ErrBadIdent");
        public static string ERR_BAD_LOCAL_VARIABLE_FMT =
            (string)Application.Current.FindResource("str_ErrBadLocalVariableFmt");
        public static string ERR_BAD_LV_TABLE_FMT =
            (string)Application.Current.FindResource("str_ErrBadLvTableFmt");
        public static string ERR_BAD_RANGE =
            (string)Application.Current.FindResource("str_ErrBadRange");
        public static string ERR_BAD_SYMBOL_LABEL =
            (string)Application.Current.FindResource("str_ErrBadSymbolLabel");
        public static string ERR_BAD_SYMBOL_ST =
            (string)Application.Current.FindResource("str_ErrBadSymbolSt");
        public static string ERR_BAD_SYMREF_PART =
            (string)Application.Current.FindResource("str_ErrBadSymrefPart");
        public static string ERR_BAD_TYPE_HINT =
            (string)Application.Current.FindResource("str_ErrBadTypeHint");
        public static string ERR_BAD_VISUALIZATION_FMT =
            (string)Application.Current.FindResource("str_ErrBadVisualizationFmt");
        public static string ERR_BAD_VISUALIZATION_SET_FMT =
            (string)Application.Current.FindResource("str_ErrBadVisualizationSetFmt");
        public static string ERR_DIR_CREATE_FAILED_FMT =
            (string)Application.Current.FindResource("str_ErrDirCreateFailedFmt");
        public static string ERR_DUPLICATE_LABEL_FMT =
            (string)Application.Current.FindResource("str_ErrDuplicateLabelFmt");
        public static string ERR_FILE_COPY_FAILED_FMT =
            (string)Application.Current.FindResource("str_ErrFileCopyFailedFmt");
        public static string ERR_FILE_EXISTS_NOT_DIR_FMT =
            (string)Application.Current.FindResource("str_ErrFileExistsNotDirFmt");
        public static string ERR_FILE_GENERIC_CAPTION =
            (string)Application.Current.FindResource("str_ErrFileGenericCaption");
        public static string ERR_FILE_NOT_FOUND_FMT =
            (string)Application.Current.FindResource("str_ErrFileNotFoundFmt");
        public static string ERR_FILE_READ_FAILED_FMT =
            (string)Application.Current.FindResource("str_ErrFileReadFailedFmt");
        public static string ERR_FILE_READ_ONLY_FMT =
            (string)Application.Current.FindResource("str_ErrFileReadOnlyFmt");
        public static string ERR_INVALID_ADDRESS_MASK =
            (string)Application.Current.FindResource("str_ErrInvalidAddressMask");
        public static string ERR_INVALID_CMP_ADDR_OVERLAP =
            (string)Application.Current.FindResource("str_ErrInvalidCmpAddrOverlap");
        public static string ERR_INVALID_CMP_EXTRA_BITS =
            (string)Application.Current.FindResource("str_ErrInvalidCmpExtraBits");
        public static string ERR_INVALID_COMPARE_MASK =
            (string)Application.Current.FindResource("str_ErrInvalidCompareMask");
        public static string ERR_INVALID_COMPARE_VALUE =
            (string)Application.Current.FindResource("str_ErrInvalidCompareValue");
        public static string ERR_INVALID_INT_VALUE =
            (string)Application.Current.FindResource("str_ErrInvalidIntValue");
        public static string ERR_INVALID_KEY_VALUE =
            (string)Application.Current.FindResource("str_ErrInvalidKeyValue");
        public static string ERR_INVALID_MULTI_MASK =
            (string)Application.Current.FindResource("str_ErrInvalidMultiMask");
        public static string ERR_INVALID_WIDTH =
            (string)Application.Current.FindResource("str_ErrInvalidWidth");
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
        public static string ERR_VALUE_INCOMPATIBLE_WITH_MASK =
            (string)Application.Current.FindResource("str_ErrValueIncompatibleWithMask");
        public static string EXPORTING_HTML =
            (string)Application.Current.FindResource("str_ExportingHtml");
        public static string EXPORTING_HTML_AND_IMAGES =
            (string)Application.Current.FindResource("str_ExportingHtmlAndImages");
        public static string EXTERNAL_FILE_BAD_DIR_FMT =
            (string)Application.Current.FindResource("str_ExternalFileBadDirFmt");
        public static string EXTERNAL_FILE_BAD_DIR_CAPTION =
            (string)Application.Current.FindResource("str_ExternalFileBadDirCaption");
        public static string FILE_FILTER_ALL =
            (string)Application.Current.FindResource("str_FileFilterAll");
        public static string FILE_FILTER_CS =
            (string)Application.Current.FindResource("str_FileFilterCs");
        public static string FILE_FILTER_CSV =
            (string)Application.Current.FindResource("str_FileFilterCsv");
        public static string FILE_FILTER_DIS65 =
            (string)Application.Current.FindResource("str_FileFilterDis65");
        public static string FILE_FILTER_GIF =
            (string)Application.Current.FindResource("str_FileFilterGif");
        public static string FILE_FILTER_HTML =
            (string)Application.Current.FindResource("str_FileFilterHtml");
        public static string FILE_FILTER_SGEC =
            (string)Application.Current.FindResource("str_FileFilterSgec");
        public static string FILE_FILTER_SYM65 =
            (string)Application.Current.FindResource("str_FileFilterSym65");
        public static string FILE_FILTER_TEXT =
            (string)Application.Current.FindResource("str_FileFilterText");
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
        public static string INFO_AUTO_FORMAT =
            (string)Application.Current.FindResource("str_InfoAutoFormat");
        public static string INFO_CUSTOM_FORMAT =
            (string)Application.Current.FindResource("str_InfoCustomFormat");
        public static string INFO_DEFAULT_FORMAT =
            (string)Application.Current.FindResource("str_InfoDefaultFormat");
        public static string INFO_LABEL_DESCR_FMT =
            (string)Application.Current.FindResource("str_InfoLabelDescrFmt");
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
        public static string MSG_BANK_OVERRUN =
            (string)Application.Current.FindResource("str_MsgBankOverrun");
        public static string MSG_BANK_OVERRUN_DETAIL_FMT =
            (string)Application.Current.FindResource("str_MsgBankOverrunDetailFmt");
        public static string MSG_FORMAT_DESCRIPTOR_IGNORED =
            (string)Application.Current.FindResource("str_MsgFormatDescriptorIgnored");
        public static string MSG_HIDDEN_LABEL =
            (string)Application.Current.FindResource("str_MsgHiddenLabel");
        public static string MSG_HIDDEN_LOCAL_VARIABLE_TABLE =
            (string)Application.Current.FindResource("str_MsgHiddenLocalVariableTable");
        public static string MSG_HIDDEN_VISUALIZATION =
            (string)Application.Current.FindResource("str_MsgHiddenVisualization");
        public static string MSG_INVALID_DESCRIPTOR =
            (string)Application.Current.FindResource("str_MsgInvalidDescriptor");
        public static string MSG_INVALID_OFFSET_OR_LENGTH =
            (string)Application.Current.FindResource("str_MsgInvalidOffsetOrLength");
        public static string MSG_LABEL_IGNORED =
            (string)Application.Current.FindResource("str_MsgLabelIgnored");
        public static string MSG_LOCAL_VARIABLE_TABLE_IGNORED =
            (string)Application.Current.FindResource("str_MsgLocalVariableTableIgnored");
        public static string MSG_UNRESOLVED_WEAK_REF =
            (string)Application.Current.FindResource("str_MsgUnresolvedWeakRef");
        public static string MSG_VISUALIZATION_IGNORED =
            (string)Application.Current.FindResource("str_MsgVisualizationIgnored");
        public static string NO_FILES_AVAILABLE =
            (string)Application.Current.FindResource("str_NoFilesAvailable");
        public static string NO_EXPORTED_SYMBOLS_FOUND =
            (string)Application.Current.FindResource("str_NoExportedSymbolsFound");
        public static string OMF_SEG_COMMENT_FMT =
            (string)Application.Current.FindResource("str_OmfSegCommentFmt");
        public static string OMF_SEG_HDR_COMMENT_FMT =
            (string)Application.Current.FindResource("str_OmfSegHdrCommentFmt");
        public static string OMF_SEG_NOTE_FMT =
            (string)Application.Current.FindResource("str_OmfSegNoteFmt");
        public static string OMF_SELECT_FILE =
            (string)Application.Current.FindResource("str_OmfSelectFile");
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
        public static string OPEN_DATA_TOO_SMALL_FMT =
            (string)Application.Current.FindResource("str_OpenDataTooSmallFmt");
        public static string OPEN_DATA_WRONG_CRC_FMT =
            (string)Application.Current.FindResource("str_OpenDataWrongCrcFmt");
        public static string OPEN_DATA_WRONG_LENGTH_FMT =
            (string)Application.Current.FindResource("str_OpenDataWrongLengthFmt");
        public static string OPERATION_FAILED =
            (string)Application.Current.FindResource("str_OperationFailed");
        public static string OPERATION_SUCCEEDED =
            (string)Application.Current.FindResource("str_OperationSucceeded");
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
        public static string PROJECT_FIELD_DBR_VALUE =
            (string)Application.Current.FindResource("str_ProjectFieldDbrValue");
        public static string PROJECT_FIELD_LONG_COMMENT =
            (string)Application.Current.FindResource("str_ProjectFieldLongComment");
        public static string PROJECT_FIELD_LV_TABLE =
            (string)Application.Current.FindResource("str_ProjectFieldLvTable");
        public static string PROJECT_FIELD_NOTE =
            (string)Application.Current.FindResource("str_ProjectFieldNote");
        public static string PROJECT_FIELD_OPERAND_FORMAT =
            (string)Application.Current.FindResource("str_ProjectFieldOperandFormat");
        public static string PROJECT_FIELD_RELOC_DATA =
            (string)Application.Current.FindResource("str_ProjectFieldRelocData");
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
        public static string STATUS_BYTE_COUNT_FMT =
            (string)Application.Current.FindResource("str_StatusByteCountFmt");
        public static string STATUS_READY =
            (string)Application.Current.FindResource("str_StatusReady");
        public static string STR_VFY_DCI_MIXED_DATA =
            (string)Application.Current.FindResource("str_StrVfyDciMixedData");
        public static string STR_VFY_DCI_NOT_TERMINATED =
            (string)Application.Current.FindResource("str_StrVfyDciNotTerminated");
        public static string STR_VFY_DCI_SHORT =
            (string)Application.Current.FindResource("str_StrVfyDciShort");
        public static string STR_VFY_L1_LENGTH_MISMATCH =
            (string)Application.Current.FindResource("str_StrVfyL1LengthMismatch");
        public static string STR_VFY_L2_LENGTH_MISMATCH =
            (string)Application.Current.FindResource("str_StrVfyL2LengthMismatch");
        public static string STR_VFY_MISSING_NULL_TERM =
            (string)Application.Current.FindResource("str_StrVfyMissingNullTerm");
        public static string STR_VFY_NULL_INSIDE_NULL_TERM =
            (string)Application.Current.FindResource("str_StrVfyNullInsideNullTerm");
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
        public static string TITLE_READ_ONLY =
            (string)Application.Current.FindResource("str_TitleReadOnly");
        public static string UNSET =
            (string)Application.Current.FindResource("str_Unset");
        public static string VIS_SET_MULTIPLE_FMT =
            (string)Application.Current.FindResource("str_VisSetMultipleFmt");
        public static string VIS_SET_SINGLE_FMT =
            (string)Application.Current.FindResource("str_VisSetSingleFmt");
    }
}

namespace DD_WorkTab.Miscellaneous
{
	public enum SortOrder : byte
	{
		Undefined,
		Descending,
		Ascending
	}

	public enum WorkFunction : byte
	{
		AllPawns_EnableWork,
		AllPawns_DisableWork,
		AllPawns_ResetWork,
		EnableWork,
		DisableWork,
		ResetWork,
		CopySettings,
		PasteSettings
	}
}

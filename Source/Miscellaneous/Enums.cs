namespace DD_WorkTab.Miscellaneous
{
	public enum SortOrder
	{
		Undefined,
		Descending,
		Ascending
	}

	public enum WorkSound
	{
		TaskCompleted,
		TaskFailed,
		WorkEnabled,
		WorkDisabled,
		SortedSkills,
		UnsortedSkills,
		CompareSkillsMapChanged
	}

	public enum WorkFunction
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

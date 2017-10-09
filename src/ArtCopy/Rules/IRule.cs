namespace ArtCopy.Rules
{
	interface IRule
	{
		FileCopyInstructions MatchFile(string path);
	}
}

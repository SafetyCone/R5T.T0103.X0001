using System;

using R5T.D0082.T001;
using R5T.T0021;
using R5T.T0041;
using R5T.T0044;
using R5T.T0107;
using R5T.T0108;


namespace R5T.T0103.X0001
{
    public static class Instances
    {
        public static ICommitMessage CommitMessage { get; } = T0107.CommitMessage.Instance;
        public static IFileName FileName { get; } = T0021.FileName.Instance;
        public static IFileSystemOperator FileSystemOperator { get; } = T0044.FileSystemOperator.Instance;
        public static IGitHubOrganization GitHubOrganization { get; } = D0082.T001.GitHubOrganization.Instance;
        public static IGitHubRepositorySpecificationGenerator GitHubRepositorySpecificationGenerator { get; } = D0082.T001.GitHubRepositorySpecificationGenerator.Instance;
        public static IPathOperator PathOperator { get; } = T0041.PathOperator.Instance;
        public static IRepositoryNameOperator RepositoryNameOperator { get; } = T0108.RepositoryNameOperator.Instance;
    }
}

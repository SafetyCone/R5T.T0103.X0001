using System;
using System.Threading.Tasks;

using R5T.D0037;
using R5T.D0082;
using R5T.T0010;
using R5T.T0104;
using R5T.T0103;
using R5T.T0106;

using Instances = R5T.T0103.X0001.Instances;


namespace System
{
    public static class IRepositoryGeneratorExtensions
    {
        /// <summary>
        /// Selects <see cref="CreateRepositoryWithCheckin(IRepositoryGenerator, string, string, string, string, IGitHubOperator, IGitOperator, Func{ILocalRepositoryContext, Task})"/> as the default.
        /// </summary>
        public static async Task CreateRepository(this IRepositoryGenerator _,
            RepositorySpecification repositorySpecification,
            string sourceControlParentDirectoryPath,
            string gitIgnoreTemplateFilePath,
            IGitHubOperator gitHubOperator,
            IGitOperator gitOperator,
            Func<ILocalRepositoryContext, Task> localRepositoryContextAction = default)
        {
            await _.CreateRepositoryWithCheckin(
                repositorySpecification,
                sourceControlParentDirectoryPath,
                gitIgnoreTemplateFilePath,
                gitHubOperator,
                gitOperator,
                localRepositoryContextAction);
        }

        public static async Task CreateRepositoryWithCheckin(this IRepositoryGenerator _,
            RepositorySpecification repositorySpecification,
            string sourceControlParentDirectoryPath,
            string gitIgnoreTemplateFilePath,
            string initialCommitMessage,
            IGitHubOperator gitHubOperator,
            IGitOperator gitOperator,
            Func<ILocalRepositoryContext, Task> localRepositoryContextAction = default)
        {
            var repositoryDirectoryPath = await _.CreateRepositoryWithoutCheckin(
                repositorySpecification,
                sourceControlParentDirectoryPath,
                gitIgnoreTemplateFilePath,
                gitHubOperator,
                gitOperator,
                localRepositoryContextAction);

            // Now perform check-in.
            await gitOperator.CheckIn(
                new LocalRepositoryDirectoryPath(repositoryDirectoryPath),
                initialCommitMessage);
        }

        public static async Task CreateRepositoryWithCheckin(this IRepositoryGenerator _,
            RepositorySpecification repositorySpecification,
            string sourceControlParentDirectoryPath,
            string gitIgnoreTemplateFilePath,
            IGitHubOperator gitHubOperator,
            IGitOperator gitOperator,
            Func<ILocalRepositoryContext, Task> localRepositoryContextAction = default)
        {
            await _.CreateRepositoryWithCheckin(
                repositorySpecification,
                sourceControlParentDirectoryPath,
                gitIgnoreTemplateFilePath,
                Instances.CommitMessage.InitialSourceStubCommit(),
                gitHubOperator,
                gitOperator,
                localRepositoryContextAction);
        }

        /// <summary>
        /// Creates a remote repository and clones it to a local directory path.
        /// Returns the repository directory path.
        /// </summary>
        /// <returns>The repository directory path.</returns>
        public static async Task<string> CreateRepositoryWithoutCheckin(this IRepositoryGenerator _,
            RepositorySpecification repositorySpecification,
            string sourceControlParentDirectoryPath,
            string gitIgnoreTemplateFilePath,
            IGitHubOperator gitHubOperator,
            IGitOperator gitOperator,
            Func<ILocalRepositoryContext, Task> localRepositoryContextAction = default)
        {
            var repositoryDirectoryName = Instances.RepositoryNameOperator.GetRepositoryDirectoryName(repositorySpecification.Name);
            var repositoryDirectoryPath = Instances.PathOperator.GetDirectoryPath(sourceControlParentDirectoryPath, repositoryDirectoryName);

            // Create repository.
            var gitHubRepositorySpecification = Instances.GitHubRepositorySpecificationGenerator.GetSafetyConeDefault(
                repositorySpecification.Name,
                repositorySpecification.Description,
                repositorySpecification.IsPrivate);

            await gitHubOperator.CreateRepository(gitHubRepositorySpecification);

            // Clone repository locally.
            var cloneUrl = await gitHubOperator.GetRepositoryCloneUrl(
                Instances.GitHubOrganization.SafetyCone(),
                gitHubRepositorySpecification.Name);

            await gitOperator.Clone(
                cloneUrl,
                new LocalRepositoryDirectoryPath(repositoryDirectoryPath));

            // Now modify the local repository context.
            var localRepositoryContext = new LocalRepositoryContext
            {
                Name = gitHubRepositorySpecification.Name,
                DirectoryPath = repositoryDirectoryPath,
            };

            // Copy the .gitignore template file.
            var gitignoreFilePath = Instances.PathOperator.GetFilePath(
                localRepositoryContext.DirectoryPath,
                Instances.FileName.GitIgnore());

            Instances.FileSystemOperator.CopyFile(gitIgnoreTemplateFilePath, gitignoreFilePath);

            await FunctionHelper.Run(localRepositoryContextAction, localRepositoryContext);

            return repositoryDirectoryPath;
        }

        /// <summary>
        /// Create a local repository (directory) without creating a remote (GitHub) repository or even registering the repository with source control.
        /// </summary>
        public static async Task<string> CreateLocalRepositoryDirectoryOkIfExists(this IRepositoryGenerator _,
            string repositoryName,
            string repositoriesDirectoryPath,
            Func<ILocalRepositoryContext, Task> localRepositoryContextAction = default)
        {
            var repositoryDirectoryName = Instances.RepositoryNameOperator.GetRepositoryDirectoryName(repositoryName);
            var repositoryDirectoryPath = Instances.PathOperator.GetDirectoryPath(repositoriesDirectoryPath, repositoryDirectoryName);

            // Ensure the local repository directory exists.
            Instances.FileSystemOperator.CreateDirectoryOkIfExists(repositoryDirectoryPath);

            // Create the local repository context, and run the provided context action.
            var localRepositoryContext = new LocalRepositoryContext
            {
                DirectoryPath = repositoryDirectoryPath,
                Name = repositoryName,
            };

            await FunctionHelper.Run(localRepositoryContextAction, localRepositoryContext);

            return repositoryDirectoryPath;
        }
    }
}

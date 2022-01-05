using System;
using System.Threading.Tasks;

using R5T.D0082;
using R5T.T0103;

using Instances = R5T.T0103.X0001.Instances;


namespace System
{
    public static class IRepositoryOperatorExtensions
    {
        public static async Task DeleteRepositoryOkIfNotExists(this IRepositoryOperator _,
            IGitHubOperator gitHubOperator,
            string repositoryName,
            int delayAfterDeletionMilliseconds = 5000)
        {
            var repositoryExists = await gitHubOperator.RepositoryExists(
                Instances.GitHubOrganization.SafetyCone(),
                repositoryName);

            if (repositoryExists)
            {
                await gitHubOperator.DeleteRepositoryNonIdempotent(
                    Instances.GitHubOrganization.SafetyCone(),
                    repositoryName);

                // Take a beat.
                await Task.Delay(delayAfterDeletionMilliseconds);
            }
        }
    }
}

using SpaceWeatherAPI.CustomQueryParameters;
using SpaceWeatherAPI.Models;
using System.Linq.Expressions;

namespace SpaceWeatherAPI.Extensions
{
    public static class BaseModelExtensions
    {
        /// <summary>
        /// Applies pagination to the query based on the provided parameters.
        /// </summary>
        /// <param name="query">The queryable object.</param>
        /// <param name="parameters">The pagination parameters.</param>
        public static void ApplyPaginationWithExtension(this IQueryable<BaseModel> query, QueryParameters parameters)
        {
            parameters.ValidationPageParams();

            query = query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize);

        }

        /// <summary>
        /// Applies a search filter to the query based on the provided parameters.
        /// </summary>
        /// <param name="query">The queryable object.</param>
        /// <param name="parameters">The search parameters.</param>
        public static void ApplySearchFilterWithExtension(this IQueryable<BaseModel> query, QueryParameters parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(x => x.Name.Contains(parameters.SearchTerm) ||
                x.WeatherInfo.Condition.ToString().Contains(parameters.SearchTerm));
            }
        }

        /// <summary>
        /// Applies ordering to the query based on the provided parameters.
        /// </summary>
        /// <param name="query">The queryable object.</param>
        /// <param name="parameters">The ordering parameters.</param>
        public static void ApplyOrderingWithExtension(this IQueryable<BaseModel> query, QueryParameters parameters)
        {
            if (parameters.GetOrder().ToLower() == "desc")
            {
                query = query.OrderByDescending(GetSortProperty<BaseModel>(parameters.GetColumn()));
            }
            else
            {
                query = query.OrderBy(GetSortProperty<BaseModel>(parameters.GetColumn()));

            }
        }

        /// <summary>
        /// Gets the sorting property based on the specified sorting column for a generic type.
        /// </summary>
        /// <typeparam name="T">Type of the object to sort.</typeparam>
        /// <param name="sortingColumn">Column to use for sorting.</param>
        /// <returns>Expression defining the property to be used for sorting.</returns>
        private static Expression<Func<T, object>> GetSortProperty<T>(string? sortingColumn) where T : BaseModel
        {
            return sortingColumn?.ToLower() switch
            {
                "name" => model => model.Name,
                "temperature" => model => ((BaseModel)model).WeatherInfo.Temperature,
                _ => model => model.Id
            };
        }
    }
}

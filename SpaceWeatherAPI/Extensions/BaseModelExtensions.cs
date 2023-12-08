using SpaceWeatherAPI.CustomQueryParameters;
using SpaceWeatherAPI.Models;
using System.Linq;
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
        public static IQueryable<T> ApplyPaginationWithExtension<T>(this IQueryable<T> query, QueryParameters parameters) where T : BaseModel
        {

            query = query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize);
            return query;

        }

        /// <summary>
        /// Applies a search filter to the query based on the provided parameters.
        /// </summary>
        /// <param name="query">The queryable object.</param>
        /// <param name="parameters">The search parameters.</param>
        public static IQueryable<T> ApplySearchFilterWithExtension<T>(this IQueryable<T> query, QueryParameters parameters) where T : BaseModel 
        {
            
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query =  query.Where(x => x.Name.ToLower().Contains(parameters.SearchTerm.ToLower()) ||
                x.WeatherInfo.Condition.ToString().ToLower().Contains(parameters.SearchTerm.ToLower()));
            }
            return query;
        }

        /// <summary>
        /// Applies ordering to the query based on the provided parameters.
        /// </summary>
        /// <param name="query">The queryable object.</param>
        /// <param name="parameters">The ordering parameters.</param>
        public static IQueryable<T> ApplyOrderingWithExtension<T>(this IQueryable<T> query, QueryParameters parameters) where T : BaseModel
        {
            if (!string.IsNullOrWhiteSpace(parameters.Sorting))
            {
                var sorting = parameters.Sorting.Split(','); // Split Columns
                
                foreach (var column in sorting)
                {
                    var parameter = column.Split('_');//Split Column _ Order
                    if (parameter.Count() > 1 && parameter[1] == "desc")
                    {
                        query = query.OrderByDescending(GetSortProperty<T>(parameter[0]));
                    }
                    else
                    {
                        query = query.OrderBy(GetSortProperty<T>(parameter[0]));

                    }
                }
            }
            else
                query.OrderBy(x => x.Id);

            return query;
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

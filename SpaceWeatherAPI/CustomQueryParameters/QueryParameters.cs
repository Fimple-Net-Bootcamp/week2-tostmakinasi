using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;

namespace SpaceWeatherAPI.CustomQueryParameters
{
    /// <param name="SearchTerm"> Sets the search term for filtering results.
    /// **Filter On Columns:** Name, Condition </param>
    /// <param name="Sorting"> Sets the sorting column and order.**Order Types:** "asc", "desc" **For Example:** "name_asc, id_desc,condition"</param>
    /// <param name="PageNumber"> Sets the page number. </param>
    /// <param name="PageSize"> Sets the page size. </param>
    public record class QueryParameters(string SearchTerm = "", string Sorting ="", int PageNumber = 1, int PageSize = 10)
    {

        
    }
}

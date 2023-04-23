using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using NP_parsing.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace NP_parsing.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public ObservableCollection<ParsingObject> DataParsing { get; set; } = new ObservableCollection<ParsingObject>();
        public ActionCommand LoadCommand => new ActionCommand(async c => await ParsingUsingSelectors());
      
        private async Task ParsingPage()
        {
            try
            {
                using HttpClient client = new HttpClient();

                using HttpResponseMessage responsePage = await client.GetAsync("https://proglib.io/vacancies/all");

                string bodyPage = await responsePage.Content.ReadAsStringAsync();
                string patternPage = "data-total=\"(.+)\"";
                Regex regexPage = new Regex(patternPage);
                Match matchPages = regexPage.Match(bodyPage);

                int pageCount = Convert.ToInt32(matchPages.Groups[1].Value);
                                
                ObservableCollection<string> adressPages = new ObservableCollection<string>();
                string path = "https://proglib.io/vacancies/all?workType=all&workPlace=all&experience=&salaryFrom=&page=";

                for (int i = 0; i < pageCount; i++) adressPages.Add($"{path}{i}");
               
                var contentPages = await Task.WhenAll( adressPages.Select(async x => await client.GetStringAsync(x)));                

                foreach (var page in contentPages) 
                {
                    string patternNameVac = "<div itemprop=\"description\">(.+)</div>";
                    string patternAdressVac = "<a href=\"(.+)\" class=\"no-link\"";
                    string patternDoCVac = "<div itemprop=\"datePosted\">(.+)</div>";
                    Regex regex = new Regex(patternNameVac);
                    MatchCollection matchesNameVac = regex.Matches(page);

                    regex = new Regex(patternAdressVac);
                    MatchCollection matchesAdressVac = regex.Matches(page);

                    regex = new Regex(patternDoCVac);
                    MatchCollection matchesDoCVac = regex.Matches(page);

                    for (int j = 0; j < matchesNameVac.Count; j++)
                    {
                        DataParsing.Add(new ParsingObject { NameVacancy = matchesNameVac[j].Groups[1].Value,
                            Adress = matchesAdressVac[j].Groups[1].Value, DateOfCreate = matchesDoCVac[j].Groups[1].Value });
                    }
                }                     
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);                
            } 
        }

        private async Task ParsingUsingSelectors()
        {
            try
            {
                var config = Configuration.Default.WithDefaultLoader();
                var context = BrowsingContext.New(config);
                var uri = "https://proglib.io/vacancies/all";

                var document = await context.OpenAsync(uri);
                var selectorCountPages = "main > div.feed-pagination";
                var res = document.QuerySelectorAll(selectorCountPages);
                var r = res.Last().TextContent;
                int pageCount = Convert.ToInt32(r.Last());                


                ObservableCollection<string> adressPages = new ObservableCollection<string>();
                string path = "https://proglib.io/vacancies/all?workType=all&workPlace=all&experience=&salaryFrom=&page=";

                for (int i = 0; i < pageCount; i++) adressPages.Add($"{path}{i}");

                var contentPages = await Task.WhenAll(adressPages.Select(async x => await context.OpenAsync(x)));

                foreach (var page in contentPages)
                {                    
                    string patternAdressVac = "article > div > div > a";
                    string patternDoCVac = "div.preview-card__publish > div.publish-info";
                                        
                    var adressVacansies = document.QuerySelectorAll(patternAdressVac);
                    var hrefVacansies = adressVacansies.Select(i =>((IHtmlAnchorElement)i).Href).ToList();
                    var docVacansies = document.QuerySelectorAll(patternDoCVac);

                    for (int i = 0; i < adressVacansies.Count(); i++)
                    {
                        DataParsing.Add(new ParsingObject
                        {                            
                            NameVacancy = adressVacansies[i].FirstElementChild.TextContent,
                            Adress = hrefVacansies[i],
                            DateOfCreate = docVacansies[i].TextContent
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        
    }
}

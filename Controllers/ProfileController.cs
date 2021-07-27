using AngleSharp;
using AngleSharp.Dom;
using FreelancerProfile.DTO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProfile.Controllers
{
    [Route("api/profile")]
    [ApiController]
    public class ProfileController : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string username)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var address = "https://www.freelancer.mx/u/" + username;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(address);
            var reviews = GetReviews(document);
            var userProfile = GetUserProfile(document);
            userProfile.UserName = username;
            userProfile.Reviews = reviews;
            userProfile.InvitationLink = $"https://www.freelancer.mx/get/{username}?f=give";
            // var profile = JsonConvert.SerializeObject(userProfile);
            // TODO: Save as file using username
            // TODO: If username exits as file, return file
            return StatusCode(200, userProfile);
        }

        private Profile GetUserProfile(IDocument document)
        {
            try
            {
                var reviewCounts = document.QuerySelectorAll(".ReviewCount");
                var reviewCount = reviewCounts.FirstOrDefault();
                var reviewCountNumber = TextToLong(reviewCount.TextContent);

                var name = Clean(document.QuerySelectorAll(".Username-displayName").FirstOrDefault().FirstElementChild.TextContent);
                var flag = document.QuerySelector("fl-link[fltrackinglabel='SummaryCountryLink']").FirstElementChild.FirstElementChild.FirstElementChild.FirstElementChild.FirstElementChild.GetAttribute("src");
                var image = document.QuerySelectorAll(".UserAvatar-img").FirstOrDefault().GetAttribute("src");

                var valueBlocks = document.QuerySelectorAll(".ValueBlock");
                var valueBlock = valueBlocks.FirstOrDefault();
                var rating = TextToDouble(valueBlock.TextContent);

                var reputationItems = document.QuerySelectorAll(".ReputationItem");
                var reputation = GetReputation(reputationItems);

                var costPerHour = document.QuerySelectorAll(".UserSummary");
                var usersumaryitem = costPerHour.FirstOrDefault().QuerySelector("fl-col").FirstElementChild.QuerySelectorAll("fl-col");
                var valuePerHour = usersumaryitem.ElementAt(1);
                var costText = valuePerHour.FirstElementChild.FirstElementChild.FirstElementChild.FirstElementChild.FirstElementChild.TextContent;
                var cost = TextToDouble(costText);

                var skillitems = document.QuerySelectorAll("app-user-profile-skills-item");

                List<string> topskills = new List<string>();
                foreach (var skillitem in skillitems)
                {
                    var skill = skillitem.FirstElementChild.FirstElementChild.FirstElementChild.FirstElementChild.FirstElementChild.FirstElementChild.TextContent;
                    skill = Clean(skill);
                    topskills.Add(skill);
                }

                return new Profile
                {
                    Rating = rating,
                    RatingCount = reviewCountNumber,
                    Reputation = reputation,
                    CostPerHour = cost,
                    Name = name,
                    Flag = flag,
                    Image = image,
                    Letter = name.FirstOrDefault().ToString(),
                    TopSkills = topskills
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }

        private List<ClientReview> GetReviews(IDocument document)
        {
            List<ClientReview> reviews = new List<ClientReview>();
            try
            {
                var reviewItems = document.QuerySelectorAll("app-review-item");

                foreach (var reviewItem in reviewItems)
                {

                    var card = reviewItem.FirstElementChild.FirstElementChild;

                    var review = TextToDouble(card.FirstElementChild.FirstElementChild.FirstElementChild.LastElementChild.TextContent);

                    var amount = TextToDouble(card.FirstElementChild.LastElementChild.FirstElementChild.FirstElementChild.TextContent);

                    var reviewItemRow = reviewItem.QuerySelectorAll(".ReviewItem-row");
                    var commentItem = reviewItemRow.ElementAt(2);
                    var comment = Clean(commentItem.FirstElementChild.FirstElementChild.TextContent);

                    var skillsItem = reviewItemRow.ElementAt(3);
                    var links = skillsItem.QuerySelectorAll("fl-tag[fltrackinglabel=SkillLink]");
                    List<string> skills = new List<string>();
                    foreach (var link in links)
                    {
                        var skill = link.FirstElementChild.FirstElementChild.FirstElementChild.FirstElementChild.FirstElementChild.FirstElementChild.TextContent;
                        skills.Add(Clean(skill));
                    }

                    var clientProfile = reviewItemRow.ElementAt(4);
                    var clientImage = clientProfile.FirstElementChild.FirstElementChild.FirstElementChild.FirstElementChild.FirstElementChild.GetAttribute("src");
                    var letter = string.Empty;
                    if (string.IsNullOrEmpty(clientImage))
                    {
                        letter = Clean(clientProfile.QuerySelector(".UserAvatarLetter").TextContent);
                    }

                    var clientFlag = clientProfile.LastElementChild.FirstElementChild.FirstElementChild.FirstElementChild.FirstElementChild.GetAttribute("src");
                    var clientName = Clean(clientProfile.LastElementChild.FirstElementChild.FirstElementChild.LastElementChild.FirstElementChild.TextContent);
                    var clientUserName = Clean(clientProfile.QuerySelector(".Username-displayName").FirstElementChild.TextContent);

                    var reviewDto = new ClientReview
                    {
                        Rating = review,
                        PaidOut = amount,
                        Comment = comment,
                        Skills = skills,
                        Profile = new Profile
                        {
                            Image = clientImage,
                            Letter = letter,
                            Flag = clientFlag,
                            Name = clientName,
                            UserName = clientUserName
                        }
                    };
                    reviews.Add(reviewDto);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return reviews;
        }

        private List<Reputation> GetReputation(IHtmlCollection<IElement> collection)
        {
            var reputationItems = new List<Reputation>();
            foreach (var item in collection)
            {
                var percentText = item.FirstElementChild.FirstElementChild.TextContent;
                var percent = TextToLong(percentText);
                var description = item.LastElementChild.FirstElementChild.TextContent;
                reputationItems.Add(new Reputation
                {
                    Description = Clean(description),
                    Percent = (int) percent
                });
            }
            return reputationItems;
        }

        /// <summary>
        /// Remueve el espacio del inicio y fin de un texto
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        private string Clean(string description)
        {
            return description?.TrimStart()?.TrimEnd()?.Trim() ?? string.Empty;
        }

        private double TextToDouble(string textContent)
        {
            string newstr = "";
            foreach (var item in textContent)
            {
                if (char.IsWhiteSpace(item))
                    continue;
                if (char.IsLetter(item) && (item.ToString() != "," || item.ToString() != "."))
                    continue;
                if (item.ToString() == ",")
                {
                    newstr += ".";
                }
                else
                {
                    if (char.IsNumber(item))
                    {
                        newstr += item;
                    }
                }
            }
            double.TryParse(newstr, out double rating);
            return rating;
        }

        private long TextToLong(string textContent)
        {
            var strnumber = "";
            foreach (var character in textContent)
            {
                if (!char.IsNumber(character)) 
                    continue;
                strnumber += character;
            }
            return long.Parse(strnumber);
        }
    }
}

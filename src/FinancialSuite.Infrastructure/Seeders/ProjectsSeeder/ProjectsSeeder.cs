using FinancialSuite.Domain.Entities;
using FinancialSuite.Infrastructure.Persistence;

namespace FinancialSuite.Infrastructure.Seeders.ProjectsSeeder
{
    internal class ProjectsSeeder(MyApplicationDbContext dbContext) : IProjectsSeeder
    {
        private const string PrimaryOwnerEmail = "superadmin@example.com";
        private const string SecondaryOwnerEmail = "admin@example.com";

        public async Task Seed()
        {
            if (await dbContext.Database.CanConnectAsync())
            {
                var primaryOwnerId = dbContext.Users.Where(x => x.Email == PrimaryOwnerEmail).Select(x => x.Id).FirstOrDefault();
                var secondaryOwnerId = dbContext.Users.Where(x => x.Email == SecondaryOwnerEmail).Select(x => x.Id).FirstOrDefault();

                // Projects.OwnerId is a required FK to AspNetUsers, so skip until both owners exist.
                if (primaryOwnerId == null || secondaryOwnerId == null) return;

                // Insert only seed projects whose names are not in the table yet, so new seed entries reach existing databases.
                var existingNames = dbContext.Projects.Select(x => x.Name).ToHashSet();
                var projects = GetProjects(primaryOwnerId, secondaryOwnerId)
                    .Where(x => !existingNames.Contains(x.Name))
                    .ToList();

                if (projects.Count == 0) return;

                dbContext.Projects.AddRange(projects);
                await dbContext.SaveChangesAsync();
            }
        }

        private IEnumerable<Project> GetProjects(string primaryOwnerId, string secondaryOwnerId)
        {
            var dateTime = DateTime.Now;

            return new List<Project>
            {
                CreateProject("Personal Portfolio Website", "Responsive portfolio site showcasing projects, skills, and contact details.", primaryOwnerId, dateTime),
                CreateProject("Budget Tracker", "Monthly income and expense tracking with category-based spending reports.", primaryOwnerId, dateTime),
                CreateProject("Recipe Organizer", "Collection of favorite recipes with ingredients, steps, and meal planning.", primaryOwnerId, dateTime),
                CreateProject("Home Inventory", "Catalog of household items with purchase dates, warranties, and receipts.", primaryOwnerId, dateTime),
                CreateProject("Reading List", "Track books to read, currently reading, and finished with personal notes.", primaryOwnerId, dateTime),
                CreateProject("Workout Planner", "Weekly exercise routines with sets, reps, and progress history.", primaryOwnerId, dateTime),
                CreateProject("Travel Itinerary Builder", "Plan trips with destinations, bookings, and daily schedules.", primaryOwnerId, dateTime),
                CreateProject("Garden Journal", "Log planting dates, watering schedules, and harvest yields.", primaryOwnerId, dateTime),
                CreateProject("Car Maintenance Log", "Record oil changes, repairs, and upcoming service reminders.", primaryOwnerId, dateTime),
                CreateProject("Learning Roadmap", "Structured plan for courses, certifications, and study milestones.", primaryOwnerId, dateTime),
                CreateProject("Movie Watchlist", "Keep track of movies to watch along with ratings and reviews.", secondaryOwnerId, dateTime),
                CreateProject("Habit Tracker", "Daily habit check-ins with streaks and weekly summaries.", secondaryOwnerId, dateTime),
                CreateProject("Event Planner", "Organize birthdays and gatherings with guest lists and task checklists.", secondaryOwnerId, dateTime),
                CreateProject("Meal Prep Scheduler", "Plan weekly meal prep sessions with shopping lists and portion counts.", primaryOwnerId, dateTime),
                CreateProject("Subscription Manager", "Track recurring subscriptions, renewal dates, and monthly costs.", primaryOwnerId, dateTime),
                CreateProject("Photo Archive", "Organize family photos by year, event, and people tagged.", primaryOwnerId, dateTime),
                CreateProject("Bill Reminder", "Due-date reminders for utilities, rent, and credit card payments.", primaryOwnerId, dateTime),
                CreateProject("Plant Care Tracker", "Watering, fertilizing, and repotting schedules for houseplants.", primaryOwnerId, dateTime),
                CreateProject("Savings Goals", "Set savings targets and track progress toward each goal.", primaryOwnerId, dateTime),
                CreateProject("Podcast Library", "Catalog favorite podcasts with episodes listened and notes.", primaryOwnerId, dateTime),
                CreateProject("Language Learning Log", "Daily vocabulary practice, lesson progress, and speaking sessions.", primaryOwnerId, dateTime),
                CreateProject("Home Renovation Plan", "Room-by-room renovation tasks, budgets, and contractor contacts.", primaryOwnerId, dateTime),
                CreateProject("Pet Health Records", "Vaccinations, vet visits, and medication schedules for pets.", primaryOwnerId, dateTime),
                CreateProject("Sleep Journal", "Record bedtime, wake time, and sleep quality each night.", primaryOwnerId, dateTime),
                CreateProject("Water Intake Tracker", "Log daily water consumption against a personal hydration goal.", primaryOwnerId, dateTime),
                CreateProject("Coffee Tasting Notes", "Notes on beans, roasts, brew methods, and flavor profiles.", primaryOwnerId, dateTime),
                CreateProject("Gift Idea List", "Collect gift ideas for friends and family throughout the year.", primaryOwnerId, dateTime),
                CreateProject("Wardrobe Catalog", "Inventory of clothing items with outfit combinations.", primaryOwnerId, dateTime),
                CreateProject("Board Game Collection", "Track owned board games, play counts, and favorite strategies.", primaryOwnerId, dateTime),
                CreateProject("Hiking Trail Log", "Record hikes with distance, elevation, and trail conditions.", primaryOwnerId, dateTime),
                CreateProject("Cycling Stats", "Ride history with distance, average speed, and routes taken.", primaryOwnerId, dateTime),
                CreateProject("Running Training Plan", "Structured training plan for a half marathon with weekly mileage.", primaryOwnerId, dateTime),
                CreateProject("Meditation Timer", "Guided meditation sessions with streaks and total minutes.", primaryOwnerId, dateTime),
                CreateProject("Journal App", "Private daily journal entries with mood tags.", primaryOwnerId, dateTime),
                CreateProject("Quote Collection", "Save inspiring quotes with author and source.", primaryOwnerId, dateTime),
                CreateProject("Bookmark Manager", "Organize saved links by topic with short descriptions.", primaryOwnerId, dateTime),
                CreateProject("Code Snippet Library", "Reusable code snippets grouped by language and purpose.", primaryOwnerId, dateTime),
                CreateProject("Side Project Ideas", "Backlog of side project ideas with effort and priority ratings.", primaryOwnerId, dateTime),
                CreateProject("Conference Notes", "Notes and takeaways from tech conferences and meetups.", primaryOwnerId, dateTime),
                CreateProject("Online Course Tracker", "Progress through online courses with completion certificates.", primaryOwnerId, dateTime),
                CreateProject("Certification Prep", "Study plan and practice exam scores for cloud certifications.", primaryOwnerId, dateTime),
                CreateProject("Resume Builder", "Maintain resume versions tailored to different job roles.", primaryOwnerId, dateTime),
                CreateProject("Job Application Tracker", "Track applications, interview stages, and follow-ups.", primaryOwnerId, dateTime),
                CreateProject("Networking Contacts", "Professional contacts with meeting notes and follow-up dates.", primaryOwnerId, dateTime),
                CreateProject("Freelance Invoices", "Create and track invoices for freelance clients.", primaryOwnerId, dateTime),
                CreateProject("Time Tracker", "Log hours spent on tasks and projects for weekly review.", primaryOwnerId, dateTime),
                CreateProject("Pomodoro Sessions", "Focus sessions with break reminders and daily totals.", primaryOwnerId, dateTime),
                CreateProject("Weekly Review", "Weekly reflection on wins, challenges, and next priorities.", primaryOwnerId, dateTime),
                CreateProject("Goal Setting Board", "Yearly goals broken down into quarterly milestones.", primaryOwnerId, dateTime),
                CreateProject("Debt Payoff Planner", "Snowball and avalanche plans for paying off debts.", primaryOwnerId, dateTime),
                CreateProject("Investment Journal", "Notes on investment decisions and portfolio rebalancing.", primaryOwnerId, dateTime),
                CreateProject("Tax Document Organizer", "Collect receipts and forms needed for annual tax filing.", primaryOwnerId, dateTime),
                CreateProject("Insurance Policy Tracker", "Policy numbers, coverage details, and renewal dates.", primaryOwnerId, dateTime),
                CreateProject("Emergency Contacts", "Important contacts and medical information for emergencies.", primaryOwnerId, dateTime),
                CreateProject("Medication Reminder", "Daily medication schedule with dosage reminders.", primaryOwnerId, dateTime),
                CreateProject("Doctor Visit Log", "Appointments, diagnoses, and prescriptions from doctor visits.", primaryOwnerId, dateTime),
                CreateProject("Grocery List", "Shared grocery list grouped by store aisle.", primaryOwnerId, dateTime),
                CreateProject("Pantry Inventory", "Track pantry items with expiration dates.", primaryOwnerId, dateTime),
                CreateProject("Wine Cellar", "Catalog wine bottles with vintage, region, and tasting notes.", primaryOwnerId, dateTime),
                CreateProject("Baking Experiments", "Log baking attempts with tweaks and results.", primaryOwnerId, dateTime),
                CreateProject("Cleaning Schedule", "Rotating household cleaning tasks by day and room.", primaryOwnerId, dateTime),
                CreateProject("Chore Chart", "Assign and track household chores among family members.", primaryOwnerId, dateTime),
                CreateProject("Moving Checklist", "Tasks, packing lists, and utility transfers for a move.", primaryOwnerId, dateTime),
                CreateProject("Music Practice Log", "Daily instrument practice with pieces and tempo goals.", secondaryOwnerId, dateTime),
                CreateProject("Song Lyrics Notebook", "Draft original song lyrics and chord progressions.", secondaryOwnerId, dateTime),
                CreateProject("Album Collection", "Catalog vinyl and CD albums with release years.", secondaryOwnerId, dateTime),
                CreateProject("Concert History", "Concerts attended with venues, dates, and setlists.", secondaryOwnerId, dateTime),
                CreateProject("TV Series Tracker", "Track seasons and episodes watched for ongoing shows.", secondaryOwnerId, dateTime),
                CreateProject("Anime Watchlist", "Anime titles to watch, in progress, and completed.", secondaryOwnerId, dateTime),
                CreateProject("Video Game Backlog", "Games owned, currently playing, and completed with play time.", secondaryOwnerId, dateTime),
                CreateProject("Comic Book Collection", "Comic issues owned with series, issue numbers, and condition.", secondaryOwnerId, dateTime),
                CreateProject("Art Portfolio", "Showcase drawings and paintings with dates and mediums.", secondaryOwnerId, dateTime),
                CreateProject("Sketchbook Prompts", "Daily drawing prompts and completed sketches.", secondaryOwnerId, dateTime),
                CreateProject("Photography Shot List", "Planned photo shoots with locations and gear lists.", secondaryOwnerId, dateTime),
                CreateProject("Blog Content Calendar", "Plan blog posts with drafts, publish dates, and topics.", secondaryOwnerId, dateTime),
                CreateProject("Newsletter Planner", "Plan newsletter issues with sections and subscriber notes.", secondaryOwnerId, dateTime),
                CreateProject("Social Media Scheduler", "Draft and schedule posts across social platforms.", secondaryOwnerId, dateTime),
                CreateProject("YouTube Channel Plan", "Video ideas, scripts, and upload schedule.", secondaryOwnerId, dateTime),
                CreateProject("Writing Project", "Chapters, outlines, and word count goals for a novel.", secondaryOwnerId, dateTime),
                CreateProject("Poetry Collection", "Original poems organized by theme and date written.", secondaryOwnerId, dateTime),
                CreateProject("Short Story Drafts", "Short story ideas, drafts, and revision notes.", secondaryOwnerId, dateTime),
                CreateProject("Family Tree", "Genealogy records with relatives, dates, and relationships.", secondaryOwnerId, dateTime),
                CreateProject("Birthday Reminders", "Birthdays and anniversaries with reminder notifications.", secondaryOwnerId, dateTime),
                CreateProject("Wedding Planner", "Vendors, budget, guest list, and timeline for a wedding.", secondaryOwnerId, dateTime),
                CreateProject("Baby Milestones", "Record first words, steps, and growth measurements.", secondaryOwnerId, dateTime),
                CreateProject("School Schedule", "Class schedules, assignments, and exam dates.", secondaryOwnerId, dateTime),
                CreateProject("Homework Tracker", "Homework assignments with due dates and completion status.", secondaryOwnerId, dateTime),
                CreateProject("Study Flashcards", "Flashcard decks for spaced repetition study.", secondaryOwnerId, dateTime),
                CreateProject("Science Fair Project", "Hypothesis, experiment steps, and results for a science fair.", secondaryOwnerId, dateTime),
                CreateProject("Volunteer Hours Log", "Record volunteer activities and hours contributed.", secondaryOwnerId, dateTime),
                CreateProject("Donation Tracker", "Charitable donations with receipts for tax purposes.", secondaryOwnerId, dateTime),
                CreateProject("Neighborhood Watch", "Community announcements and incident reports.", secondaryOwnerId, dateTime),
                CreateProject("Book Club Organizer", "Book club picks, meeting dates, and discussion questions.", secondaryOwnerId, dateTime),
                CreateProject("Sports League Stats", "Team rosters, game scores, and player statistics.", secondaryOwnerId, dateTime),
                CreateProject("Fantasy Football Notes", "Draft rankings, trades, and weekly lineup decisions.", secondaryOwnerId, dateTime),
                CreateProject("Fishing Log", "Fishing trips with location, catch, and weather conditions.", secondaryOwnerId, dateTime),
                CreateProject("Camping Gear Checklist", "Gear lists and packing checklists for camping trips.", secondaryOwnerId, dateTime),
                CreateProject("Road Trip Planner", "Routes, stops, fuel costs, and accommodations for road trips.", secondaryOwnerId, dateTime),
                CreateProject("Passport and Visa Tracker", "Travel document expiry dates and visa requirements.", secondaryOwnerId, dateTime),
                CreateProject("Packing List Templates", "Reusable packing lists for different trip types.", secondaryOwnerId, dateTime),
                CreateProject("Currency Converter Notes", "Exchange rates and spending notes for trips abroad.", secondaryOwnerId, dateTime),
                CreateProject("Restaurant Reviews", "Restaurants visited with dishes, ratings, and prices.", secondaryOwnerId, dateTime),
                CreateProject("Cocktail Recipes", "Cocktail recipes with ingredients and garnish notes.", secondaryOwnerId, dateTime),
                CreateProject("Smart Home Setup", "Devices, automations, and network configuration for the home.", secondaryOwnerId, dateTime),
                CreateProject("Home Network Map", "Router settings, devices, and IP assignments.", secondaryOwnerId, dateTime),
                CreateProject("PC Build Planner", "Component list, prices, and compatibility notes for a PC build.", secondaryOwnerId, dateTime),
                CreateProject("Gadget Warranty Tracker", "Electronics purchases with warranty expiry dates.", secondaryOwnerId, dateTime),
                CreateProject("3D Printing Projects", "Print files, filament settings, and print results.", secondaryOwnerId, dateTime),
                CreateProject("DIY Woodworking", "Woodworking plans, materials, and build progress.", secondaryOwnerId, dateTime),
                CreateProject("Knitting Patterns", "Knitting patterns, yarn stash, and project progress.", secondaryOwnerId, dateTime),
                CreateProject("Sewing Projects", "Sewing patterns, fabric inventory, and measurements.", secondaryOwnerId, dateTime),
                CreateProject("Bird Watching Log", "Bird species spotted with location and date.", secondaryOwnerId, dateTime),
                CreateProject("Astronomy Observations", "Night sky observations with telescope settings and targets.", secondaryOwnerId, dateTime),
            };
        }

        private Project CreateProject(string name, string description, string ownerId, DateTime dateTime)
        {
            var creator = "Seeder";

            return new Project
            {
                Name = name,
                Description = description,
                OwnerId = ownerId,
                CreatedBy = creator,
                DateCreated = dateTime,
                UpdatedBy = "",
                DateUpdated = dateTime,
                IsActive = true
            };
        }
    }
}

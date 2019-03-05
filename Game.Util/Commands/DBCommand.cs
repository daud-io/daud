namespace Game.Util.Commands
{
    using Google.Cloud.Firestore;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    class DBCommand : CommandBase
    {
        /*[Required, Argument(0, Description = "contextName")]
        public string ContextName { get; }*/

        protected override async Task ExecuteAsync()
        {
            var projectID = "daud-230416";
            var firestore = FirestoreDb.Create(projectID);
            var eventsRef = firestore.Collection("events");
            var query = eventsRef.WhereEqualTo("GameID", "5bb172bd7072454f8389f9be8cffc441");
            var querySnapshot = await query.GetSnapshotAsync();
            var dictionary = querySnapshot.Documents.Select(d => d.ToDictionary());

            Table("Documents", dictionary);
        }
    }
}
//
//
// using Cdms.Backend.Data;
// using Cdms.Business.Pipelines.Matching;
// using Cdms.Business.Services;
// using Cdms.Model;
// using Cdms.Model.Alvs;
// using Cdms.Model.Auditing;
// using Cdms.Model.Ipaffs;
// using FluentAssertions;
// using MediatR;
// using MongoDB.Driver;
// using Xunit;
// using Document = Cdms.Model.Alvs.Document;
// using Items = Cdms.Model.Alvs.Items;
//
// namespace Cdms.Business.Tests.Services;
//
// public class LinkingServiceTests
// {
//     private static readonly Random random = new ();
//     private static readonly MongoDbContext dbContext;
//     private static string GenerateDocumentReference(int id) => $"GBCVD2024.{id}";
//     private static string GenerateNotificationReference(int id) => $"CHEDP.GB.2024.{id}";
//     
//     static LinkingServiceTests()
//     {
//         var client = new MongoClient(MongoRunnerProvider.Instance.Get().ConnectionString);
//         var db = client.GetDatabase($"Cdms_MongoDb_{Guid.NewGuid()}_Test");
//         dbContext = new MongoDbContext(db);
//     }
//
//     [Fact]
//     public async Task LinkClearanceRequest_ExistingRequest_IncludesFieldsOfInterest_NewerThanCurrentVersion_MatchingCHEDs_MRNSaved_AddsAllToMatchContext()
//     {
//         // Arrange
//         var testData = await AddTestData();
//         int unknownChedReference = GenerateRandomReference();
//
//         var sut = new LinkingService(dbContext);
//         var clearanceRequest = CreateClearanceRequest(testData.Movements[0], [testData.Cheds[0], unknownChedReference], 4);
//
//         // Act
//         var matchCtx = await sut.Link(clearanceRequest);
//
//         // Assert
//         matchCtx.Should().NotBeNull();
//         matchCtx.ContinueMatching.Should().BeTrue();
//         matchCtx.Notifications.Count.Should().Be(1);
//         matchCtx.Movements.Count.Should().Be(1);
//     }
//     
//     [Fact]
//     public async Task LinkClearanceRequest_ExistingRequest_IncludesFieldsOfInterest_NewerThanCurrentVersion_NoMatchingCHEDs_MRNSaved_NoMatchingTriggered()
//     {
//         // Arrange
//         var testData = await AddTestData();
//         int unknownChedReference = GenerateRandomReference();
//
//         var sut = new LinkingService(dbContext);
//         var clearanceRequest = CreateClearanceRequest(testData.Movements[0], [unknownChedReference], 4);
//         
//         // Act
//         var matchCtx = await sut.Link(clearanceRequest);
//         
//         // Assert
//         matchCtx.Should().NotBeNull();
//         matchCtx.ContinueMatching.Should().BeFalse();
//         matchCtx.Notifications.Count.Should().Be(0);
//         matchCtx.Movements.Count.Should().Be(0);
//     }
//
//     [Fact]
//     public async Task LinkClearanceRequest_ExistingRequest_IncludesFieldsOfInterest_OlderThanCurrentVersion_MRNNotSaved_NoMatchingTriggered()
//     {
//         // Arrange
//         var testData = await AddTestData();
//         
//         var sut = new LinkingService(dbContext);
//         var clearanceRequest = CreateClearanceRequest(testData.Movements[0], [testData.Cheds[0]], 2);
//         
//         // Act
//         var matchCtx = await sut.Link(clearanceRequest);
//         
//         // Assert
//         matchCtx.Should().NotBeNull();
//         matchCtx.ContinueMatching.Should().BeFalse();
//         matchCtx.Notifications.Count.Should().Be(0);
//         matchCtx.Movements.Count.Should().Be(0);
//     }
//
//     [Fact]
//     public async Task LinkClearanceRequest_ExistingRequest_NoFieldsOfInterest_MRNSaved_NoMatchingTriggered()
//     {
//         // Arrange
//         var testData = await AddTestData();
//         
//         var sut = new LinkingService(dbContext);
//         var clearanceRequest = CreateClearanceRequest(testData.Movements[0], [testData.Cheds[0]], 4);
//         
//         // Act
//         var matchCtx = await sut.Link(clearanceRequest);
//         
//         // Assert
//         matchCtx.Should().NotBeNull();
//         matchCtx.ContinueMatching.Should().BeFalse();
//         matchCtx.Notifications.Count.Should().Be(0);
//         matchCtx.Movements.Count.Should().Be(0);
//     }
//
//     [Fact]
//     public async Task LinkClearanceRequest_NewRequest_MatchingCHED_MRNSaved_AddsAllToMatchContext()
//     {
//         // Arrange
//         var testData = await AddTestData();
//         var unknownEntryReference = $"TESTREF{GenerateRandomReference()}";
//
//         var sut = new LinkingService(dbContext);
//         var clearanceRequest = CreateClearanceRequest(unknownEntryReference, [testData.Cheds[0]], 4);
//         
//         // Act
//         var matchCtx = await sut.Link(clearanceRequest);
//         
//         // Assert
//         matchCtx.Should().NotBeNull();
//         matchCtx.ContinueMatching.Should().BeTrue();
//         matchCtx.Notifications.Count.Should().Be(1);
//         matchCtx.Movements.Count.Should().Be(1);
//     }
//     
//     [Fact]
//     public async Task LinkClearanceRequest_NewRequest_MultipleMatchingCHEDs_MRNSaved_AddsAllToMatchContext()
//     {
//         // Arrange
//         var testData = await AddTestData(2, 1, 2);
//         var unknownEntryReference = $"TEST{GenerateRandomReference()}";
//
//         var sut = new LinkingService(dbContext);
//         var clearanceRequest = CreateClearanceRequest(unknownEntryReference, [testData.Cheds[0], testData.Cheds[1]], 4);
//         
//         // Act
//         var matchCtx = await sut.Link(clearanceRequest);
//         
//         // Assert
//         matchCtx.Should().NotBeNull();
//         matchCtx.ContinueMatching.Should().BeTrue();
//         matchCtx.Notifications.Count.Should().Be(2);
//         matchCtx.Movements.Count.Should().Be(1);
//     }
//     
//     [Fact]
//     public async Task LinkClearanceRequest_NewRequest_NoMatchingCHEDs_MRNSaved_NoMatchingTriggered()
//     {
//         // Arrange
//         int unknownChedReference = GenerateRandomReference();
//         var unknownEntryReference = $"TEST{GenerateRandomReference()}";
//
//         var sut = new LinkingService(dbContext);
//         var clearanceRequest = CreateClearanceRequest(unknownEntryReference, [unknownChedReference], 4);
//         
//         // Act
//         var matchCtx = await sut.Link(clearanceRequest);
//         
//         // Assert
//         matchCtx.Should().NotBeNull();
//         matchCtx.ContinueMatching.Should().BeFalse();
//         matchCtx.Notifications.Count.Should().Be(0);
//         matchCtx.Movements.Count.Should().Be(0);
//     }
//     
//     [Fact]
//     public async Task LinkNotification_ExistingCHED_IncludesFieldsOfInterest_NewerThanCurrentVersion_MatchingMRN_CHEDSaved_AddsAllToMatchContext()
//     {
//         // Arrange
//         var testData = await AddTestData();
//
//         var sut = new LinkingService(dbContext);
//         var notification = CreateNotification(testData.Cheds[0]);
//         
//         // Act
//         var matchCtx = await sut.Link(notification);
//         
//         // Assert
//         matchCtx.Should().NotBeNull();
//         matchCtx.ContinueMatching.Should().BeTrue();
//         matchCtx.Notifications.Count.Should().Be(1);
//         matchCtx.Movements.Count.Should().Be(1);
//     }
//
//     [Fact]
//     public async Task LinkNotification_ExistingCHED_IncludesFieldsOfInterest_NewerThanCurrentVersion_MultipleMatchingMRNs_CHEDSaved_AddsAllToMatchContext()
//     {
//         // Arrange
//         var testData = await AddTestData(2,2,2);
//         
//         var sut = new LinkingService(dbContext);
//         var notification = CreateNotification(testData.Cheds[0]);
//         
//         // Act
//         var matchCtx = await sut.Link(notification);
//         
//         // Assert
//         matchCtx.Should().NotBeNull();
//         matchCtx.ContinueMatching.Should().BeTrue();
//         matchCtx.Notifications.Count.Should().Be(1);
//         matchCtx.Movements.Count.Should().Be(2);
//     }
//     
//     [Fact]
//     public async Task LinkNotification_ExistingCHED_IncludesFieldsOfInterest_NewerThanCurrentVersion_NoMatchingMRNs_CHEDSaved_NoMatchingTriggered()
//     {
//         // Arrange
//         var testData = await AddTestData(1, 1, 0);
//         
//         var sut = new LinkingService(dbContext);
//         var notification = CreateNotification(testData.Cheds[0]);
//         
//         // Act
//         var matchCtx = await sut.Link(notification);
//         
//         // Assert
//         matchCtx.Should().NotBeNull();
//         matchCtx.ContinueMatching.Should().BeFalse();
//         matchCtx.Notifications.Count.Should().Be(0);
//         matchCtx.Movements.Count.Should().Be(0);
//     }
//
//     [Fact]
//     public async Task LinkNotification_ExistingCHED_IncludesFieldsOfInterest_OlderThanCurrentVersion_CHEDNotSaved_NoMatchingTriggered()
//     {
//         // Arrange
//         var testData = await AddTestData();
//         
//         var sut = new LinkingService(dbContext);
//         var notification = CreateNotification(testData.Cheds[0], true, DateTime.UtcNow.AddDays(-1));
//         
//         // Act
//         var matchCtx = await sut.Link(notification);
//         
//         // Assert
//         matchCtx.Should().NotBeNull();
//         matchCtx.ContinueMatching.Should().BeFalse();
//         matchCtx.Notifications.Count.Should().Be(0);
//         matchCtx.Movements.Count.Should().Be(0);
//     }
//
//     [Fact]
//     public async Task LinkNotification_ExistingCHED_NoFieldsOfInterest_CHEDSaved_NoMatchingTriggered()
//     {
//         // Arrange
//         var testData = await AddTestData(2, 2, 2);
//         
//         var sut = new LinkingService(dbContext);
//         var notification = CreateNotification(testData.Cheds[0], false, DateTime.UtcNow);
//         
//         // Act
//         var matchCtx = await sut.Link(notification);
//         
//         // Assert
//         matchCtx.Should().NotBeNull();
//         matchCtx.ContinueMatching.Should().BeFalse();
//         matchCtx.Notifications.Count.Should().Be(0);
//         matchCtx.Movements.Count.Should().Be(0);
//     }
//
//     [Fact]
//     public async Task LinkNotification_NewCHED_MatchingMRN_CHEDSaved_AddsAllToMatchContext()
//     {
//         // Arrange
//         int newChedReference = GenerateRandomReference();
//         var newEntryReference = $"TEST{GenerateRandomReference()}";
//         
//         var clearanceRequest = CreateClearanceRequest(newEntryReference, [newChedReference]);
//         var notification = CreateNotification(newChedReference);
//         
//         var sut = new LinkingService(dbContext);
//
//         var matchCtxSetup = await sut.Link(clearanceRequest);
//
//         // Act
//         var matchCtxActual = await sut.Link(notification);
//
//         // Assert
//         matchCtxSetup.Should().NotBeNull();
//         matchCtxSetup.ContinueMatching.Should().BeFalse();
//         matchCtxSetup.Notifications.Count.Should().Be(0);
//         matchCtxSetup.Movements.Count.Should().Be(0);
//         matchCtxActual.Should().NotBeNull();
//         matchCtxActual.ContinueMatching.Should().BeTrue();
//         matchCtxActual.Notifications.Count.Should().Be(1);
//         matchCtxActual.Movements.Count.Should().Be(1);
//     }
//
//     [Fact]
//     public async Task LinkNotification_NewCHED_MultipleMatchingMRNs_CHEDSaved_AddsAllToMatchContext()
//     {
//         // Arrange
//         int newChedReference = GenerateRandomReference();
//         var newEntryReference = $"TEST{GenerateRandomReference()}";
//         var newEntryReference2 = $"TEST{GenerateRandomReference()}";
//         
//         var clearanceRequest = CreateClearanceRequest(newEntryReference, [newChedReference]);
//         var clearanceRequest2 = CreateClearanceRequest(newEntryReference2, [newChedReference]);
//         var notification = CreateNotification(newChedReference);
//         
//         var sut = new LinkingService(dbContext);
//
//         var matchCtxSetup = await sut.Link(clearanceRequest);
//         var matchCtxSetup2 = await sut.Link(clearanceRequest2);
//
//         // Act
//         var matchCtxActual = await sut.Link(notification);
//
//         // Assert
//         matchCtxSetup.Should().NotBeNull();
//         matchCtxSetup.ContinueMatching.Should().BeFalse();
//         matchCtxSetup.Notifications.Count.Should().Be(0);
//         matchCtxSetup.Movements.Count.Should().Be(0);
//         matchCtxSetup2.Should().NotBeNull();
//         matchCtxSetup2.ContinueMatching.Should().BeFalse();
//         matchCtxSetup2.Notifications.Count.Should().Be(0);
//         matchCtxSetup2.Movements.Count.Should().Be(0);
//         matchCtxActual.Should().NotBeNull();
//         matchCtxActual.ContinueMatching.Should().BeTrue();
//         matchCtxActual.Notifications.Count.Should().Be(1);
//         matchCtxActual.Movements.Count.Should().Be(2);
//     }
//     
//     [Fact]
//     public async Task LinkNotification_NewCHED_NoMatchingMRNs_CHEDSaved_NoMatchingTriggered()
//     {
//         // Arrange
//         int newChedReference = GenerateRandomReference();
//
//         var sut = new LinkingService(dbContext);
//         var notification = CreateNotification(newChedReference);
//         
//         // Act
//         var matchCtx = await sut.Link(notification);
//         
//         // Assert
//         matchCtx.Should().NotBeNull();
//         matchCtx.ContinueMatching.Should().BeFalse();
//         matchCtx.Notifications.Count.Should().Be(0);
//         matchCtx.Movements.Count.Should().Be(0);
//     }
//
//
//     private AlvsClearanceRequest CreateClearanceRequest(string entryReference, List<int> chedReferences, int versionNo = 1)
//     {
//         var output = new AlvsClearanceRequest()
//         {
//             ServiceHeader = new()
//             {
//                 SourceSystem = "CDS",
//                 DestinationSystem = "ALVS",
//                 CorrelationId = Guid.NewGuid().ToString(),
//             },
//             Header = new()
//             {
//                 EntryReference = entryReference,
//                 EntryVersionNumber = versionNo,
//                 DeclarationType = "F"
//             },
//             Items = chedReferences.Select(c => new Items()
//             {
//                 Documents =
//                 [
//                     new Document() { DocumentReference = GenerateDocumentReference(c) }
//                 ]
//             }).ToArray()
//         };
//         
//         return output;
//     }
//     private ImportNotification CreateNotification(int chedReferences, bool addFieldsOfInterest = true, DateTime? lastUpdated = null)
//     {
//         var output = new ImportNotification
//         {
//             LastUpdated = lastUpdated ?? DateTime.UtcNow,
//             ReferenceNumber = GenerateNotificationReference(chedReferences),
//             Commodities = addFieldsOfInterest ? 
//             [ new CommodityComplement()
//                 {
//                     CommodityId = "1234567",
//                     CommodityDescription = "Definitely real things"
//                 }
//             ] : []
//         };
//         return output;
//     }
//
//     private async Task<(List<int> Cheds, List<string> Movements)> AddTestData(int chedCount = 1, int movementCount = 1, int matchedChedsPerMovement = 1)
//     {
//         matchedChedsPerMovement = int.Min(matchedChedsPerMovement, chedCount);
//         var movements = new List<string>();
//         var cheds = new List<int>();
//         
//         
//         for (int i = 0; i < chedCount; i++)
//         {
//             var matchingRef = GenerateRandomReference();
//             cheds.Add(matchingRef);
//             
//             await dbContext.Notifications.Insert(new ImportNotification()
//             {
//                 LastUpdated = DateTime.UtcNow.AddHours(-1),
//                 ReferenceNumber = GenerateNotificationReference(matchingRef),
//                 Commodities = []
//             });
//         }
//
//         for (int i = 0; i < movementCount; i++)
//         {
//             var entryRef = $"TESTREF{GenerateRandomReference()}";
//
//             movements.Add(entryRef);
//             
//             var mov = new Movement()
//             {
//                 Id = entryRef,
//                 ClearanceRequests =
//                 [
//                     new AlvsClearanceRequest
//                     {
//                         Header = new() { EntryReference = entryRef, EntryVersionNumber = 3, DeclarationType = "F" }
//                     }
//                 ],
//                 Items = new ()
//             };
//             
//             for (int j = 0; j < matchedChedsPerMovement; j++)
//             {
//                 mov.Items.Add(
//                     new Items()
//                     {
//                         Documents =
//                         [
//                             new Document() { DocumentReference = GenerateDocumentReference(cheds[j]) }
//                         ]
//                     });
//             }
//
//             await dbContext.Movements.Insert(mov);
//         }
//         return (cheds, movements);
//     }
//     
//     private static int GenerateRandomReference()
//     {
//         string intString = "1";
//         
//         for (int i = 0; i < 6; i++)
//         {
//             intString += random.Next(9).ToString();
//         }
//         
//         return int.Parse(intString);
//     }
// }
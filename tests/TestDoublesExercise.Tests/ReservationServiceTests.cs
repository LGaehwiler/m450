using Moq;

namespace TestDoublesExercise.Tests;

public sealed class ReservationServiceTests
{

    private sealed class Stub(IReadOnlyDictionary<string, int> availableSeatsByEventCode) : ISeatAvailability
    {
        public Stub(string eventCode, int availableSeats)
            : this( new Dictionary<string, int> { [eventCode] = availableSeats })
        {
        }
        
        public int CallCount { get; private set; }

        public int GetAvailableSeats(string eventCode)
        {
            CallCount++;    
            return availableSeatsByEventCode.GetValueOrDefault(eventCode, 0);
        }
    }

    private sealed class Repo(bool trySave = true) : IReservationRepository
    {
        public List<Reservation> savedReservation { get; } = [];
        public bool TrySave(Reservation reservation)
        {
            savedReservation.Add(reservation);
            return trySave;
        }
    }

    private sealed class Spionier : IConfirmationSender
    {
        private readonly List<Reservation> _sentReservation = [];
        public IReadOnlyList<Reservation> SentReservations => _sentReservation;
        public void Send(Reservation reservation)
        {
            _sentReservation.Add(reservation);
        }
    }
    
    // Entfernt Skip jeweils erst, wenn ihr den Test implementiert habt.
    // Die Beispiele für Stub, Spy und Moq stehen in TestDoubleExamples.cs.

    [Fact]
    public void Reserve_WithEnoughSeats_ReturnsTrue()
    {
        
        // Arrange: Stub mit 3 freien Plätzen, Repository mit erfolgreichem Speichern,
        //          eigener Sender-Spy und ReservationService.
        var availability = new Stub("Test 1", 3);
        var repository = new Repo();
        var sender = new Spionier();
        var service = new ReservationService(availability, repository, sender);
        
        // Act:     2 Plätze reservieren.

        var result = service.Reserve("Simon.DwaynoX@gmail.com", "Test 1", 2);
        // Assert:  Ergebnis ist true.
        Assert.True(result);
    }

    [Fact]
    public void Reserve_WithTooFewSeats_DoesNotSaveOrSend()
    {
        // Arrange: Stub mit 1 freiem Platz und Doubles für Repository und Sender.
        var availability = new Stub("Test 1", 1);
        var repository = new Repo();
        var sender = new Spionier();
        var service = new ReservationService(availability, repository, sender);
        // Act:     2 Plätze reservieren.
        var result = service.Reserve("Simon.DwaynoX@gmail.com", "Test 1", 2);
        // Assert:  Ergebnis ist false; weder Speichern noch Senden wurde aufgerufen.
        Assert.False(result);
    }

    [Fact(Skip = "TODO: Grenzfall testen")]
    public void Reserve_WithExactlyEnoughSeats_Succeeds()
    {
        // Arrange: Stub mit genau 2 freien Plätzen.
        var availability = new Stub("Test 1", 2);
        var repository = new Repo();
        var sender = new Spionier();
        var service = new ReservationService(availability, repository, sender);
        // Act:     2 Plätze reservieren.
        var result = service.Reserve("Simon.DwaynoX@gmail.com", "Test 1", 2);
        // Assert:  Ergebnis ist true.
        Assert.True(result);
    }

    [Fact(Skip = "TODO: Platzanzahl 0 testen")]
    public void Reserve_WithZeroSeats_ThrowsBeforeCallingDependencies()
    {
        // Arrange: Doubles, deren Aufrufe sich prüfen lassen.
        var availability = new Stub("Test 1", 3);
        var repository = new Repo();
        var sender = new Spionier();
        var service = new ReservationService(availability, repository, sender);
        // Act:     0 Plätze reservieren.
        Action act = () => service.Reserve("Simon.DwaynoX@gmail.com", "Test 1", 0);
        // Assert:  ArgumentOutOfRangeException; keine Abhängigkeit wurde aufgerufen.
        Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal(0, availability.CallCount);
        Assert.Empty(repository.savedReservation);
        Assert.Empty(sender.SentReservations);
    }

    [Fact(Skip = "TODO: negative Platzanzahl testen")]
    public void Reserve_WithNegativeSeats_ThrowsBeforeCallingDependencies()
    {
        // Arrange: Doubles, deren Aufrufe sich prüfen lassen.
        // Act:     -1 Plätze reservieren.
        // Assert:  ArgumentOutOfRangeException; keine Abhängigkeit wurde aufgerufen.
        throw new NotImplementedException();
    }

    [Fact(Skip = "TODO: leeren Veranstaltungscode testen")]
    public void Reserve_WithEmptyEventCode_ThrowsBeforeCallingDependencies()
    {
        // Arrange: Doubles, deren Aufrufe sich prüfen lassen.
        // Act:     Mit leerem Veranstaltungscode reservieren.
        // Assert:  ArgumentException; keine Abhängigkeit wurde aufgerufen.
        throw new NotImplementedException();
    }

    [Fact(Skip = "TODO: E-Mail-Adresse aus Leerzeichen testen")]
    public void Reserve_WithWhitespaceEmail_ThrowsBeforeCallingDependencies()
    {
        // Arrange: Handgeschriebene Doubles oder Moq gezielt auswählen.
        // Act:     Mit einer E-Mail-Adresse aus Leerzeichen reservieren.
        // Assert:  ArgumentException; keine Abhängigkeit wurde aufgerufen.
        throw new NotImplementedException();
    }

    [Fact(Skip = "TODO: fehlgeschlagenes Speichern testen")]
    public void Reserve_WhenSavingFails_DoesNotSendConfirmation()
    {
        // Arrange: Stub mit genügend Plätzen, Repository liefert false, eigener Spy.
        // Act:     Reservierung versuchen.
        // Assert:  Ergebnis ist false und der Spy hat keinen Aufruf aufgezeichnet.
        throw new NotImplementedException();
    }

    [Fact]
    public void Reserve_WithTooFewSeats_DoesNotCallRepository()
    {
        // Arrange: Stub mit zu wenigen Plätzen und Mock<IReservationRepository>.
        var availability = new Stub("Test 1", 1);
        var repository = new Mock<IReservationRepository>();
        var sender = new  Spionier();
        var service = new ReservationService(availability, repository.Object, sender);
        
        // Act:     Reservierung versuchen.
        var result = service.Reserve("Nokohashi.Samote@gmail.com","Test 1", 2);
        // Assert:  Verify(..., Times.Never()) für TrySave.
        Assert.False(result);
        repository.Verify(r => r.TrySave(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact(Skip = "Fortgeschrittene: ReserveBatch zuerst implementieren")]
    public void ReserveBatch_WhenCombinedSeatsExceedCapacity_DoesNotSaveOrSend()
    {
        // Arrange: Zwei Reservierungen für dieselbe Veranstaltung, die einzeln passen.
        // Act:     Nach Implementierung des Batch-Verfahrens beide zusammen reservieren.
        // Assert:  Ergebnis ist false; kein Batch-Speichern und keine Bestätigung.
        throw new NotImplementedException();
    }
}

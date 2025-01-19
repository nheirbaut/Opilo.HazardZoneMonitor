﻿using Opilo.HazardZoneMonitor.Domain.Entities;
using Opilo.HazardZoneMonitor.Domain.Events;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class PersonTests : IDisposable
{
    Person? _person;

    public PersonTests()
    {
        DomainEvents.Reset();
    }

    [Fact]
    public void Create_GivenValidParameters_CreatesValidPerson()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(0, 0);
        var timeout = TimeSpan.FromSeconds(1);

        // Act
        _person = Person.Create(personId, location, timeout);

        // Assert
        Assert.NotNull(_person);
        Assert.Equal(personId, _person.Id);
        Assert.Equal(location, _person.Location);
    }

    [Fact]
    public async Task Create_GivenValidParameters_RaisesPersonCreatedEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(0, 0);
        var timeout = TimeSpan.FromSeconds(5);

        var personCreatedEventTask = DomainEventsExtensions.Register<PersonCreatedEvent>();

        // Act
        _person = Person.Create(personId, location, timeout);
        var personCreatedEvent = await personCreatedEventTask;

        // Assert
        Assert.NotNull(personCreatedEvent);
        Assert.Equal(personId, personCreatedEvent.Person.Id);
        Assert.Equal(location, personCreatedEvent.Person.Location);
    }

    [Fact]
    public async Task UpdateLocation_WithNewLocation_RaisesPersonLocationChangedEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var initialLocation = new Location(0, 0);
        var newLocation = new Location(1, 1);
        var timeout = TimeSpan.FromSeconds(1);

        var personLocationChangedEventTask = DomainEventsExtensions.Register<PersonLocationChangedEvent>();
        _person = Person.Create(personId, initialLocation, timeout);

        // Act
        _person.UpdateLocation(newLocation);
        var personLocationChangedEvent = await personLocationChangedEventTask;

        // Assert
        Assert.NotNull(personLocationChangedEvent);
        Assert.Equal(personId, personLocationChangedEvent.Person.Id);
        Assert.Equal(newLocation, personLocationChangedEvent.Person.Location);
    }

    [Fact]
    public async Task ExpirePerson_WhenTimeExpires_RaisesPersonExpiredEvent()
    {
        // Arrange
        var lifespanTimeout = TimeSpan.FromMilliseconds(10);
        var personId = Guid.NewGuid();
        var location = new Location(0, 0);

        var personExpiredEventTask = DomainEventsExtensions.Register<PersonExpiredEvent>();
        _person = Person.Create(personId, location, lifespanTimeout);

        // Act
        var personExpiredEvent = await personExpiredEventTask;

        // Assert
        Assert.NotNull(personExpiredEvent);
        Assert.Equal(personId, personExpiredEvent.Person.Id);
    }

    public void Dispose()
    {
        _person?.Dispose();
        _person = null;
    }
}

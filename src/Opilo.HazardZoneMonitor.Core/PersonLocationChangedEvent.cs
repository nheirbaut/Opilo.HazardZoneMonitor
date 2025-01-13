// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Opilo.HazardZoneMonitor.Core;

public record PersonLocationChangedEvent(Person Person, Location PreviousLocation, Location NewLocation) : IDomainEvent;

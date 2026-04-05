GameService: eligible Reservations -> the reservations shouldnt be hardcoded into the game query service. if i add a new reservation then i would have to add it there as well, thats bad practice. find another way where that gets passed automatically

similarily, in reservationKind.cs all reservations are hardcoded outside the domain layer -> that shouldnt be the case. maybe we should rework the way the reservations in the domain layer for the reservations to be a record (?) saving reservation type and priority, would that work?


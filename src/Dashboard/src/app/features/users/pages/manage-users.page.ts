import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-manage-users-page',
  template: '<main class="manage-users-page"><h1>Manage Users</h1></main>',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageUsersPage {}

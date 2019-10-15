import { Component, OnInit, Input } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { UserGroup } from 'src/app/shared/models/system/userGroup.model';

@Component({
  selector: 'form-user-group',
  templateUrl: './form-user-group.component.html'
})
export class FormUserGroupComponent extends PopupBase implements OnInit {
  @Input() title: string = '';
  users: any[] = [];
  levelPemissions: any[] = [];
  positions: any[] = [];
  permissions: any[] = [];
  userGroup: UserGroup = null;

  constructor() {
    super();
  }

  ngOnInit() {
  }

}

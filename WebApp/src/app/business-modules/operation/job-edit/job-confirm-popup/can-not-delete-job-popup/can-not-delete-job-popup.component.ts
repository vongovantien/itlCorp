import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
  selector: 'app-can-not-delete-job-popup',
  templateUrl: './can-not-delete-job-popup.component.html'
})
export class CanNotDeleteJobPopupComponent extends PopupBase implements OnInit {

  constructor() {
    super();
  }

  ngOnInit() {
  }
  close() {
    this.hide();
  }
}

import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
  selector: 'app-add-more-modal',
  templateUrl: './add-more-modal.component.html'
})
export class AddMoreModalComponent extends PopupBase implements OnInit {

  constructor() {
    super();
  }

  ngOnInit() {
  }

}

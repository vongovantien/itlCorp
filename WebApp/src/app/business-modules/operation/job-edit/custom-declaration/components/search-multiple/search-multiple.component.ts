import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
  selector: 'app-search-multiple',
  templateUrl: './search-multiple.component.html',
  styleUrls: ['./search-multiple.component.scss']
})
export class SearchMultipleComponent extends PopupBase implements OnInit {
  @Output() isCloseModal = new EventEmitter();

  customNoSearch: string = '';
  constructor() { 
    super();
  }
  close() {
    this.customNoSearch = '';
    this.hide();
}
ApplyToList(){
  debugger;
  this.isCloseModal.emit(this.customNoSearch);
  this.hide()
}
ClearToList(){
  debugger;
  this.isCloseModal.emit('');
  this.hide()
}



  ngOnInit() {
  }

}

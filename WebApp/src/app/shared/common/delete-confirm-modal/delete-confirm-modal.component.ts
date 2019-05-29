import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-delete-confirm-modal',
  templateUrl: './delete-confirm-modal.component.html'
})
export class DeleteConfirmModalComponent implements OnInit {
  @Input() title:String;
  @Output() delete:EventEmitter<Boolean>= new EventEmitter<any>();

  constructor() { }

  ngOnInit() {
  }
  onDelete(){
    this.delete.emit(true);
  }
}

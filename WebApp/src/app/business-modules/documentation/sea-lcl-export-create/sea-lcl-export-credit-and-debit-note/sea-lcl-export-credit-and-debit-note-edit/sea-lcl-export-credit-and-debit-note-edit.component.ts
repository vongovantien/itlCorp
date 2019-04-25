import { Component, OnInit } from '@angular/core';
import { NgForm } from '@angular/forms';
declare var $: any;

@Component({
  selector: 'app-sea-lcl-export-credit-and-debit-note-edit',
  templateUrl: './sea-lcl-export-credit-and-debit-note-edit.component.html',
  styleUrls: ['./sea-lcl-export-credit-and-debit-note-edit.component.scss']
})
export class SeaLclExportCreditAndDebitNoteEditComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }
  

  viewDetailCDNote() {
    $('#edit-credit-debit-note-modal').modal('hide');
    $('#detail-credit-debit-note-modal').modal('show');
  }


  UpdateCDNote(form: NgForm) {}
}

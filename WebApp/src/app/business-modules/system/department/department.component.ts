import { Component, OnInit, ViewChild } from '@angular/core';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { Router } from '@angular/router';

@Component({
  selector: 'app-department',
  templateUrl: './department.component.html',
  styleUrls: ['./department.component.sass']
})
export class DepartmentComponent implements OnInit {
  @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

  headers: CommonInterface.IHeaderTable[];

  constructor(private _router: Router) { }

  ngOnInit() {
    this.headers = [
      { title: 'Department Code', field: 'settlementNo', sortable: true },
      { title: 'Name EN', field: 'amount', sortable: true },
      { title: 'Name Local', field: 'chargeCurrency', sortable: true },
      { title: 'Name Abbr', field: 'requester', sortable: true },
      { title: 'Office', field: 'requestDate', sortable: true },
      { title: 'Status', field: 'statusApproval', sortable: true },
    ];
  }

  showDeletePopup() {
    //this.selectedSettlement = settlement;
    this.confirmDeletePopup.show();
  }

  onSearchDepartment(data: any) {
    console.log(data);
  }

  gotoDetailDepartment(){
    this._router.navigate([`home/system/department/detail`]);
  }

}

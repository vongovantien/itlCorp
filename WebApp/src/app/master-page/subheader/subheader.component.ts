import { Component, OnInit,ViewChild,AfterViewInit } from '@angular/core';
import {PageSidebarComponent} from '../page-sidebar/page-sidebar.component';

@Component({
  selector: 'app-subheader',
  templateUrl: './subheader.component.html',
  styleUrls: ['./subheader.component.sass']
})
export class SubheaderComponent implements OnInit,AfterViewInit {

  @ViewChild(PageSidebarComponent,{static:false}) pageSideBar;

  Page_Info ={}

  ngAfterViewInit(): void {
   // throw new Error("Method not implemented.");
   // this.Page_Info = this.pageSideBar.Page_Info;
  //  / console.log(this.Page_Info);
  }

  constructor() { }

  ngOnInit() {
  }

}

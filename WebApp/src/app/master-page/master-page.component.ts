import { Component, OnInit,ViewChild,AfterViewInit } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import {PageSidebarComponent} from './page-sidebar/page-sidebar.component';


@Component({
  selector: 'app-master-page',
  templateUrl: './master-page.component.html',
  styleUrls: ['./master-page.component.css']
})
export class MasterPageComponent implements OnInit,AfterViewInit {

  @ViewChild(PageSidebarComponent) Page_side_bar;
  Page_Info ={};
  Component_name:"no-name";


  ngAfterViewInit(): void {
   this.Page_Info = this.Page_side_bar.Page_Info;
   console.log(this.Page_Info);
  }

  constructor(private baseService: BaseService) { }

  async ngOnInit() {
    var url = "https://api.github.com/repositories/19438/issues"
    var url_club = "https://gola-server.herokuapp.com/api/club/create";
    var issues = await this.baseService.getAsync(url, null, true);

    this.baseService.get(url).subscribe(data=>{
      console.log(data);
    })

    
    console.log(issues);
    console.log("hi");

  }


  MenuChanged(event){
    this.Page_Info = event;
    console.log(this.Page_Info);
    this.Component_name = event.children;
    console.log(this.Component_name);
  }


}

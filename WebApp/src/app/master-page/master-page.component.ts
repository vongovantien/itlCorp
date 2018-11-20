import { Component, OnInit,ViewChild,AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import {PageSidebarComponent} from './page-sidebar/page-sidebar.component';
import { Router } from '@angular/router';
import { SystemConstants } from 'src/constants/system.const';
import { CookieService } from 'ngx-cookie-service';
import { OAuthService } from 'angular-oauth2-oidc';

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
  // console.log(this.Page_Info);
  }

  constructor(private baseService: BaseService,private router: Router,private cdRef:ChangeDetectorRef,private cookieService: CookieService,private oauthService: OAuthService, ) { }

   ngOnInit() {
    console.log({ahahah:this.oauthService.getAccessToken()});
    if(this.oauthService.getAccessToken()==null){
      this.router.navigateByUrl('/login');
    }
    this.cdRef.detectChanges();
  }


  MenuChanged(event){
    this.Page_Info = event;      
    this.Component_name = event.children; 
  }


}

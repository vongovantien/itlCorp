import { Component, OnInit,ViewChild,AfterViewInit, ChangeDetectorRef } from '@angular/core';
import {PageSidebarComponent} from './page-sidebar/page-sidebar.component';
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { OAuthService } from 'angular-oauth2-oidc';
import { BaseService } from 'src/services-base/base.service';


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
  }

  constructor(private baseServices:BaseService,private router: Router,private cdRef:ChangeDetectorRef,private cookieService: CookieService,private oauthService: OAuthService, ) { }

   ngOnInit() {
    this.cdRef.detectChanges();
    setInterval(() => {
       var isCloseExpriteTime = this.baseServices.checkExpireTimeToken();
       var token = this.baseServices.getAccessToken();
       if(token!==null && isCloseExpriteTime){
          this.baseServices.warningToast("Phiên đăng nhập sẽ hết hạn trong 3 phút nữa, hãy lưu công việc hiện tại hoặc đăng nhập lại để tiếp tục công việc !","Cảnh Báo !")
       }
    }, 60000);
  }


  MenuChanged(event){
    this.Page_Info = event;      
    this.Component_name = event.children; 
  }
 
  logout(){
    this.oauthService.logOut(true);  
    this.router.navigateByUrl("/login");
    localStorage.clear(); 
  }


}

import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { PageSidebarComponent } from './page-sidebar/page-sidebar.component';
import { Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { BaseService } from 'src/app/shared/services/base.service';


@Component({
    selector: 'app-master-page',
    templateUrl: './master-page.component.html',
    styleUrls: ['./master-page.component.css']
})
export class MasterPageComponent implements OnInit, AfterViewInit {

    @ViewChild(PageSidebarComponent, { static: false }) Page_side_bar: { Page_Info: {}; };
    Page_Info = {};
    Component_name: "no-name";


    ngAfterViewInit(): void {
        this.Page_Info = this.Page_side_bar.Page_Info;
    }

    constructor(private baseServices: BaseService, private router: Router, private cdRef: ChangeDetectorRef, private oauthService: OAuthService, ) { }

    ngOnInit() {
        this.cdRef.detectChanges();
        setInterval(() => {
            const remainingMinutes: number = this.baseServices.remainingExpireTimeToken();

            if (!this.baseServices.checkLoginSession()) {
                this.router.navigate(['/login', { isEndSession: true }]);
            }

            if (remainingMinutes <= 3 && remainingMinutes > 0) {
                this.baseServices.warningToast("Phiên đăng nhập sẽ hết hạn sau " + remainingMinutes + " phút nữa, hãy lưu công việc hiện tại hoặc đăng nhập lại để tiếp tục công việc.", "Cảnh Báo !")
            }


        }, 15000);
    }

    MenuChanged(event: any) {
        this.Page_Info = event;
        this.Component_name = event.children;
    }

    logout() {
        this.oauthService.logOut(true);
        this.router.navigateByUrl("/login");
        localStorage.clear();
    }


}

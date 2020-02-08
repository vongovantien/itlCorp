import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { PageSidebarComponent } from './page-sidebar/page-sidebar.component';
import { Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { BaseService } from 'src/app/shared/services/base.service';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';


@Component({
    selector: 'app-master-page',
    templateUrl: './master-page.component.html',
})
export class MasterPageComponent implements OnInit, AfterViewInit {

    @ViewChild(PageSidebarComponent, { static: false }) Page_side_bar: { Page_Info: {}; };
    Page_Info = {};
    Component_name: "no-name";


    ngAfterViewInit(): void {
        this.Page_Info = this.Page_side_bar.Page_Info;
    }

    constructor(
        private baseServices: BaseService,
        private router: Router,
        private cdRef: ChangeDetectorRef,
        private oauthService: OAuthService,
        private http: HttpClient,
    ) { }

    ngOnInit() {

        this.cdRef.detectChanges();
        setInterval(() => {
            const remainingMinutes: number = this.baseServices.remainingExpireTimeToken();
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
        this.http.get(`${environment.HOST.INDENTITY_SERVER_URL}/api/Account/Signout`).toPromise()
            .then(
                (res: any) => {
                    console.log(this.oauthService);
                    this.oauthService.logOut(true);
                    this.router.navigateByUrl("/login");
                    // localStorage.clear();
                }
            );
    }
}

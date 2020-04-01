
import { Component } from '@angular/core';
import { Router, Event, NavigationStart, NavigationEnd, NavigationCancel, NavigationError } from '@angular/router';
import { NgProgress, NgProgressRef } from '@ngx-progressbar/core';
import { OAuthService } from 'angular-oauth2-oidc';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent {

    isLoading: boolean = false;
    progressRef: NgProgressRef;

    constructor(
        private router: Router,
        private _ngProgressService: NgProgress,
        private oauthService: OAuthService
    ) {
        this.progressRef = this._ngProgressService.ref();
        this.oauthService.setStorage(localStorage);
        this.oauthService.setupAutomaticSilentRefresh();
    }

    ngOnInit() {
        this.router.events.subscribe((event: Event) => {
            switch (true) {
                case event instanceof NavigationStart: {
                    this.isLoading = true;
                    this.progressRef.start();
                    break;
                }

                case event instanceof NavigationEnd:
                case event instanceof NavigationCancel:
                case event instanceof NavigationError: {
                    this.progressRef.complete();
                    break;
                }
                default: {
                    break;
                }
            }
        });
    }
}

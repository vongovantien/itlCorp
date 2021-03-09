
import { Component } from '@angular/core';
import { Router, Event, NavigationStart, NavigationEnd, NavigationCancel, NavigationError, ActivatedRoute } from '@angular/router';
import { NgProgress, NgProgressRef } from '@ngx-progressbar/core';
import { OAuthService, OAuthEvent, OAuthInfoEvent, TokenResponse } from 'angular-oauth2-oidc';
import { ToastrService } from 'ngx-toastr';
import { JwtService, SEOService } from '@services';
import { map, filter, mergeMap, tap } from 'rxjs/operators';
import { SwUpdate } from '@angular/service-worker';

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
        private oauthService: OAuthService,
        private _toast: ToastrService,
        private _jwt: JwtService,
        private _seoService: SEOService,
        private _activatedRoute: ActivatedRoute,
        private _SwUpdates: SwUpdate
    ) {
        this.progressRef = this._ngProgressService.ref();
        this.oauthService.setStorage(localStorage);
        // this.oauthService.setupAutomaticSilentRefresh();
    }

    ngOnInit() {

        // * Router Event
        this.router.events.pipe(
            tap((event: Event) => {
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
            })
        ).pipe(
            filter((event) => event instanceof NavigationEnd),
            map(() => this._activatedRoute),
            map((route) => {
                while (route.firstChild) {
                    route = route.firstChild;
                }
                return route;
            }),
            filter((route) => route.outlet === 'primary'),
            mergeMap((route) => route.data)
        ).subscribe(
            (routeData: { name: string, title: string }) => {
                this._seoService.updateTitle(routeData.title || 'eFMS');
            }
        );

        // * Oauth    
        this.oauthService.events.subscribe(
            (e: OAuthEvent) => {
                if (e instanceof OAuthInfoEvent) {
                    if (e.type === 'token_expires') {
                        this.oauthService.refreshToken().then(
                            (res: TokenResponse | any) => {
                                this._jwt.saveTokenID('efms');
                            }
                        );
                    }
                    if (e.type === 'logout') {
                        this._toast.info('successfully', e.type);
                    }
                }
            });

        // * Service Worker
        if (this._SwUpdates.isEnabled) {
            console.log("Service worker is anabled");
        }

        this._SwUpdates.available.subscribe(event => {
            console.log('current version is', event.current);
            console.log('available version is', event.available);
        });
        this._SwUpdates.activated.subscribe(event => {
            console.log('old version was', event.previous);
            console.log('new version is', event.current);
        });
    }
}

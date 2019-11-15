
import { NgModule } from "@angular/core";
import { HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { StoreModule } from "@ngrx/store";
import { EffectsModule } from "@ngrx/effects";

import { HeaderComponent } from "./master-page/header/header.component";
import { FooterComponent } from "./master-page/footer/footer.component";
import { PageSidebarComponent } from "./master-page/page-sidebar/page-sidebar.component";
import { AppComponent } from "./app.component";
import { LoginComponent } from "./login/login.component";
import { MasterPageComponent } from "./master-page/master-page.component";
import { NotfoundPageComponent } from "./notfound-page/notfound-page.component";
import { DashboardComponent } from "./dashboard/dashboard.component";

import {
    PerfectScrollbarModule,
} from "ngx-perfect-scrollbar";
import { CookieService } from "ngx-cookie-service";
import { OAuthModule } from "angular-oauth2-oidc";
import { NgxSpinnerModule } from "ngx-spinner";
import { HighchartsChartModule } from "highcharts-angular";
import { NgxDaterangepickerMd } from "ngx-daterangepicker-material";
import { NgProgressModule } from "@ngx-progressbar/core";
import { ToastrModule } from "ngx-toastr";

import { GlobalState } from "./global-state";
import { AuthInterceptor } from "./auth.interceptor";
import { AppRoutingModule } from "./app-routing.module";
import { StoreDevtoolsModule } from "@ngrx/store-devtools";
import { environment } from "src/environments/environment";
import { IdentityRepo } from "./shared/repositories";


@NgModule({
    declarations: [
        AppComponent,
        LoginComponent,
        MasterPageComponent,
        NotfoundPageComponent,
        HeaderComponent,
        FooterComponent,
        PageSidebarComponent,
        DashboardComponent
    ],
    imports: [
        CommonModule,
        BrowserModule,
        AppRoutingModule,
        FormsModule,
        HttpClientModule,
        BrowserAnimationsModule,
        ToastrModule.forRoot({
            positionClass: 'toast-bottom-right',
        }),
        NgxSpinnerModule,
        PerfectScrollbarModule,
        OAuthModule.forRoot({
            resourceServer: {
                allowedUrls: ["**"],
                sendAccessToken: true
            }
        }),
        NgxDaterangepickerMd.forRoot(),
        HighchartsChartModule,
        NgProgressModule,

        StoreModule.forRoot({}),
        EffectsModule.forRoot([]),
        StoreDevtoolsModule.instrument({
            maxAge: 25, // Retains last 25 states
            logOnly: !environment.production, // Restrict extension to log-only mode
        }),

    ],
    providers: [
        GlobalState,
        CookieService,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true,
        },
    ],

    bootstrap: [AppComponent],
    exports: []
})
export class AppModule { }


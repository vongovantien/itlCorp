import { HeaderComponent } from "./master-page/header/header.component";
import { FooterComponent } from "./master-page/footer/footer.component";
import { PageSidebarComponent } from "./master-page/page-sidebar/page-sidebar.component";
import { SubheaderComponent } from "./master-page/subheader/subheader.component";
import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { AppComponent } from "./app.component";
import { AppRoutingModule } from "./app-routing.module";
import { FormsModule } from "@angular/forms";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { LoginComponent } from "./login/login.component";
import { MasterPageComponent } from "./master-page/master-page.component";
import { NotfoundPageComponent } from "./notfound-page/notfound-page.component";
import { HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { ToastrModule } from "ngx-toastr";
import { CommonModule } from "@angular/common";
import {
    PerfectScrollbarModule,
    PerfectScrollbarConfigInterface,
    PERFECT_SCROLLBAR_CONFIG
} from "ngx-perfect-scrollbar";
import { CookieService } from "ngx-cookie-service";
import { OAuthModule } from "angular-oauth2-oidc";
import { NgxSpinnerModule } from "ngx-spinner";
import { DashboardComponent } from "./dashboard/dashboard.component";
import { HighchartsChartModule } from "highcharts-angular";
import { NgxDaterangepickerMd } from "ngx-daterangepicker-material";
import { ScrollingModule } from "@angular/cdk/scrolling";
import { AuthInterceptor } from "./auth.interceptor";
import { GlobalState } from "./global-state";

const DEFAULT_PERFECT_SCROLLBAR_CONFIG: PerfectScrollbarConfigInterface = {
    // wheelPropagation: true
};
@NgModule({
    declarations: [
        AppComponent,
        LoginComponent,
        MasterPageComponent,
        NotfoundPageComponent,
        HeaderComponent,
        FooterComponent,
        PageSidebarComponent,
        SubheaderComponent,
        DashboardComponent
    ],
    imports: [
        CommonModule,
        BrowserModule,
        AppRoutingModule,
        FormsModule,
        HttpClientModule,
        BrowserAnimationsModule,
        ToastrModule.forRoot(),
        NgxSpinnerModule,
        PerfectScrollbarModule,
        OAuthModule.forRoot({
            resourceServer: {
                allowedUrls: ["**"],
                sendAccessToken: true
            }
        }),
        NgxDaterangepickerMd,
        HighchartsChartModule,
        
    ],
    providers: [
        GlobalState,
        CookieService,
        {
            provide: PERFECT_SCROLLBAR_CONFIG, // Scrollbar
            useValue: DEFAULT_PERFECT_SCROLLBAR_CONFIG // Scrollbar
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true,
        }

    ],

    bootstrap: [AppComponent],
    exports: [ScrollingModule]
})
export class AppModule { }


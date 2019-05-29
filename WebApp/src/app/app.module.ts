
import { HeaderComponent } from './master-page/header/header.component';
import { FooterComponent } from './master-page/footer/footer.component';
import { PageSidebarComponent } from './master-page/page-sidebar/page-sidebar.component';
import { SubheaderComponent } from './master-page/subheader/subheader.component';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { FormsModule} from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpModule } from '@angular/http';
import { LoginComponent } from './login/login.component';
import { MasterPageComponent } from './master-page/master-page.component';
import { NotfoundPageComponent } from './notfound-page/notfound-page.component';
import { BaseService } from 'src/services-base/base.service';
import { HttpClientModule } from '@angular/common/http';
import { ToastrModule } from 'ngx-toastr';
import {PagingService} from './shared/common/pagination/paging-service';
import { CommonModule } from '@angular/common';
import { PerfectScrollbarModule, PerfectScrollbarConfigInterface, PERFECT_SCROLLBAR_CONFIG } from 'ngx-perfect-scrollbar';
import { SelectModule } from 'ng2-select';
import { SharedModule } from './shared/shared.module';
import { CookieService } from 'ngx-cookie-service';
import { OAuthModule } from 'angular-oauth2-oidc';
import { NgxSpinnerModule } from 'ngx-spinner';
import { NgProgressModule } from '@ngx-progressbar/core';
import { AuthGuardService } from 'src/services-base/auth-guard.service';
// import { ServiceWorkerModule } from '@angular/service-worker';
// import { environment } from '../environments/environment';
// import { ServiceWorkerModule } from '@angular/service-worker';
// import { environment } from '../environments/environment';
import { DashboardComponent } from './dashboard/dashboard.component';
import { HighchartsChartModule } from "highcharts-angular";
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { ScrollingModule } from '@angular/cdk/scrolling';
// import { ChartModule } from 'angular-highcharts';
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
    // TwoDigitDecimaNumberDirective
  ],
  imports: [    
   // ServiceWorkerModule.register('ngsw-worker.js', { enabled: true}),
    ScrollingModule,
    SharedModule,
    CommonModule,
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpModule,
    HttpClientModule,
    BrowserAnimationsModule, // required animations module
    ToastrModule.forRoot(), // ToastrModule added
    NgxSpinnerModule,
    NgProgressModule,
    PerfectScrollbarModule,
    SelectModule, // Scrollbar
    OAuthModule.forRoot(),
    NgxDaterangepickerMd,
    HighchartsChartModule,
    // ChartModule // add ChartModule to your imports
  ],
  providers: [
    AuthGuardService,
    BaseService,
    CookieService ,
    PagingService,
    { 
      provide: PERFECT_SCROLLBAR_CONFIG, // Scrollbar
      useValue: DEFAULT_PERFECT_SCROLLBAR_CONFIG // Scrollbar
    }
  ],
  bootstrap: [AppComponent],
  exports:[ScrollingModule]
})
export class AppModule { }

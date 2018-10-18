
import { HeaderComponent } from './master-page/header/header.component';
import { FooterComponent } from './master-page/footer/footer.component';
import { PageSidebarComponent } from './master-page/page-sidebar/page-sidebar.component';
import { SubheaderComponent } from './master-page/subheader/subheader.component';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpModule } from '@angular/http';
import { LoginComponent } from './login/login.component';
import { MasterPageComponent } from './master-page/master-page.component';
import { NotfoundPageComponent } from './notfound-page/notfound-page.component';
import { BaseService } from 'src/services-base/base.service';
import { HttpClientModule } from '@angular/common/http';
import { ToastrModule } from 'ngx-toastr';
import { Ng4LoadingSpinnerModule } from 'ng4-loading-spinner';
import { Daterangepicker } from 'ng2-daterangepicker';
import { PagingClientComponent } from './shared/paging-client/paging-client.component';
import {PagingService} from './shared/common/pagination/paging-service';
import { CommonModule } from '@angular/common';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    MasterPageComponent,
    NotfoundPageComponent,
    HeaderComponent,
    FooterComponent,
    PageSidebarComponent,
    SubheaderComponent
  ],
  imports: [    
    CommonModule,
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpModule,
    HttpClientModule,
    BrowserAnimationsModule, // required animations module
    ToastrModule.forRoot(), // ToastrModule added
    Ng4LoadingSpinnerModule.forRoot(),
    Daterangepicker
  ],
  providers: [BaseService,PagingService],
  bootstrap: [AppComponent],
  exports:[]
})
export class AppModule { }

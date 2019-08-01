import { NgModule } from '@angular/core';
import { ExcelService } from './excel.service';
import { SortService } from './sort.service';
import { ApiService } from './api.service';
import { PagingService } from './paging-service';
import { BaseService } from './base.service';
import { AuthGuardService } from './auth-guard.service';
import { DataService } from './data.service';

@NgModule({
    declarations: [],
    imports: [],
    exports: [],
    providers: [
        ExcelService,
        SortService,
        ApiService,
        PagingService,
        BaseService,
        AuthGuardService,
        DataService
    ],
})
export class ServiceModule { }
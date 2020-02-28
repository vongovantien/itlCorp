import { NgModule, ModuleWithProviders, Optional, SkipSelf } from '@angular/core';
import { ExcelService } from './excel.service';
import { SortService } from './sort.service';
import { ApiService } from './api.service';
import { PagingService } from './paging-service';
import { BaseService } from './base.service';
import { AuthGuardService } from './auth-guard.service';
import { DataService } from './data.service';
import { PreviousRouteService } from './previous-route';


@NgModule({
    declarations: [],
    imports: [
    ],
    exports: [],
    providers: [
        ExcelService,
        SortService,
        ApiService,
        PagingService,
        BaseService,
        AuthGuardService,
        DataService,
        PreviousRouteService,
    ],
})
export class ServiceModule {
    constructor(@Optional() @SkipSelf() parentModule: ServiceModule) {
        if (parentModule) {
            throw new Error('ServiceModule is already loaded. Import it in the AppModule only');
        }
    }
}

import { NgModule, Optional, SkipSelf } from '@angular/core';

import { PreviousRouteService, JwtService, ExcelService, SortService, ApiService, PagingService, BaseService, AuthGuardService, DataService } from '@services';


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
        JwtService
    ],
})
export class ServiceModule {
    constructor(@Optional() @SkipSelf() parentModule: ServiceModule) {
        if (parentModule) {
            throw new Error('ServiceModule is already loaded. Import it in the AppModule only');
        }
    }
}

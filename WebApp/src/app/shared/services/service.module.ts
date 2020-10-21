import { NgModule, Optional, SkipSelf } from '@angular/core';

import { PreviousRouteService, JwtService, SortService, ApiService, PagingService, DataService, SEOService } from '@services';
@NgModule({
    declarations: [],
    imports: [
    ],
    exports: [],
    providers: [
        SortService,
        ApiService,
        PagingService,
        DataService,
        PreviousRouteService,
        JwtService,
        SEOService
    ],
})
export class ServiceModule {
    constructor(@Optional() @SkipSelf() parentModule: ServiceModule) {
        if (parentModule) {
            throw new Error('ServiceModule is already loaded. Import it in the AppModule only');
        }
    }
}

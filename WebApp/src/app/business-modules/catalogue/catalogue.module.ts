import { NgModule } from '@angular/core';
import { CatalogueRoutingModule } from './catalogue-routing.module';
import { ShareCommercialModule } from '../commercial/share-commercial.module';

@NgModule({
    imports: [
        CatalogueRoutingModule,
        ShareCommercialModule
    ],
    declarations: [],
    bootstrap: []
})
export class CatalogueModule { }

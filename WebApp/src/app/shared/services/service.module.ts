import { NgModule } from '@angular/core';
import { ExcelService } from './excel.service';
import { SortService } from './sort.service';
import { ApiService } from './api.service';

@NgModule({
    declarations: [],
    imports: [],
    exports: [],
    providers: [
        ExcelService,
        SortService,
        ApiService
    ],
})
export class ServiceModule {}
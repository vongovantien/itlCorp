import { NgModule } from '@angular/core';
import { SharedModule } from 'src/app/shared/shared.module';
import { CustomClearanceModule } from './custom-clearance/custom-clearance.module';

@NgModule({
    declarations: [],
    imports: [
        SharedModule,
        CustomClearanceModule
    ],
    exports: [CustomClearanceModule]
})
export class SharedOperationModule { }

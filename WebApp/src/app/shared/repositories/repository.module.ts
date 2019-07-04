import { NgModule } from '@angular/core';
import { JobRepo } from './JobRepo.repo';

@NgModule({
    providers: [
        JobRepo
    ],
})
export class RepositoryModule {
}
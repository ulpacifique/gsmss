import api from "./client";

export interface ContributionLimit {
  limitId: number;
  goalId: number;
  goalName: string;
  fixedAmount?: number;
  minimumAmount?: number;
  maximumAmount?: number;
  maximumTotalPerUser?: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateContributionLimitRequest {
  goalId: number;
  fixedAmount?: number;
  minimumAmount?: number;
  maximumAmount?: number;
  maximumTotalPerUser?: number;
  isActive?: boolean;
}

export interface UpdateContributionLimitRequest {
  fixedAmount?: number;
  minimumAmount?: number;
  maximumAmount?: number;
  maximumTotalPerUser?: number;
  isActive?: boolean;
}

export const fetchContributionLimits = async (): Promise<ContributionLimit[]> => {
  const { data } = await api.get<ContributionLimit[]>("/api/admin/contribution-limits");
  return data;
};

export const fetchContributionLimitByGoal = async (goalId: number): Promise<ContributionLimit | null> => {
  try {
    const { data } = await api.get<ContributionLimit>(`/api/admin/contribution-limits/goal/${goalId}`);
    return data;
  } catch (error: any) {
    if (error?.response?.status === 404) {
      return null;
    }
    throw error;
  }
};

export const createContributionLimit = async (request: CreateContributionLimitRequest): Promise<ContributionLimit> => {
  const { data } = await api.post<ContributionLimit>("/api/admin/contribution-limits", request);
  return data;
};

export const updateContributionLimit = async (
  limitId: number,
  request: UpdateContributionLimitRequest
): Promise<ContributionLimit> => {
  const { data } = await api.put<ContributionLimit>(`/api/admin/contribution-limits/${limitId}`, request);
  return data;
};

export const deleteContributionLimit = async (limitId: number): Promise<void> => {
  await api.delete(`/api/admin/contribution-limits/${limitId}`);
};



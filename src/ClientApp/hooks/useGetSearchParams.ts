import {
  UnknownOutputParams,
  useGlobalSearchParams,
  useLocalSearchParams,
} from "expo-router";

function useGetSearchParams<
  TParams extends UnknownOutputParams = UnknownOutputParams,
>(): TParams {
  const localParams = useLocalSearchParams<TParams>();
  const globalParams = useGlobalSearchParams<TParams>();

  return {
    ...globalParams,
    ...localParams,
  } as TParams;
}

export default useGetSearchParams;
